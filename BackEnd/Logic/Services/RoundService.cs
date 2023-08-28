using FastEndpoints;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OhMyWord.Core.Models;
using OhMyWord.Logic.Contracts.Events;
using OhMyWord.Logic.Extensions;
using OhMyWord.Logic.Models;
using OhMyWord.Logic.Options;
using OhMyWord.Logic.Services.State;

namespace OhMyWord.Logic.Services;

public interface IRoundService
{
    /// <summary>
    /// Factory method for creating a <see cref="Round"/>.
    /// </summary>
    /// <param name="roundNumber">The round number to assign to the round.</param>
    /// <param name="sessionId">The <see cref="Session"/> ID associated with the round.</param>
    /// <param name="reloadWords">Optional flag to trigger a reload of words from the database.</param>
    /// <param name="cancellationToken">Task cancellation token.</param>
    /// <returns>A new <see cref="Round"/> object.</returns>
    Task<Round> CreateRoundAsync(int roundNumber, Guid sessionId, bool reloadWords = false,
        CancellationToken cancellationToken = default);

    Task<RoundSummary> ExecuteRoundAsync(Round round, CancellationToken cancellationToken = default);
    Task SaveRoundAsync(Round round, CancellationToken cancellationToken = default);
}

public class RoundService : IRoundService
{
    private readonly ILogger<RoundService> logger;
    private readonly IPlayerState playerState;
    private readonly IWordQueueService wordQueueService;
    private readonly IRoundsRepository roundsRepository;
    private readonly IPlayerService playerService;

    private readonly TimeSpan letterHintDelay;
    private readonly TimeSpan postRoundDelay;
    private readonly int guessLimit;

    public RoundService(ILogger<RoundService> logger, IOptions<RoundOptions> options, IPlayerState playerState,
        IWordQueueService wordQueueService, IRoundsRepository roundsRepository, IPlayerService playerService)
    {
        this.logger = logger;
        this.playerState = playerState;
        this.wordQueueService = wordQueueService;
        this.roundsRepository = roundsRepository;
        this.playerService = playerService;

        letterHintDelay = TimeSpan.FromSeconds(options.Value.LetterHintDelay);
        postRoundDelay = TimeSpan.FromSeconds(options.Value.PostRoundDelay);
        guessLimit = options.Value.GuessLimit;
    }

    public async Task<Round> CreateRoundAsync(int roundNumber, Guid sessionId, bool reloadWords = false,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Creating round {RoundNumber} for session {SessionId}", roundNumber, sessionId);
        var word = await wordQueueService.GetNextWordAsync(reloadWords, cancellationToken);
        var now = DateTime.UtcNow;

        var round = new Round
        {
            Word = word,
            WordHint = WordHint.FromWord(word),
            StartDate = now,
            EndDate = now + word.Length * letterHintDelay,
            Number = roundNumber,
            GuessLimit = guessLimit,
            SessionId = sessionId
        };

        // create player data for each player
        foreach (var playerId in playerState.PlayerIds)
            round.PlayerData[playerId] = new RoundPlayerData();

        return round;
    }

    public async Task<RoundSummary> ExecuteRoundAsync(Round round, CancellationToken cancellationToken)
    {
        logger.LogInformation("Executing round {RoundNumber} for session {SessionId}", round.Number, round.SessionId);

        try
        {
            foreach (var index in GetShuffledRange(round.Word.Length))
            {
                await Task.Delay(letterHintDelay, cancellationToken);

                var letterHint = CreateLetterHint(round.Word, index);
                round.WordHint.LetterHints.Add(letterHint);
                await PublishLetterHintAddedEventAsync(letterHint, cancellationToken);
            }
        }
        catch (TaskCanceledException)
        {
            logger.LogInformation("Round {RoundNumber} has been terminated early. Reason: {EndReason}",
                round.Number, round.EndReason);
        }

        return CreateRoundSummary(round);
    }

    public async Task SaveRoundAsync(Round round, CancellationToken cancellationToken)
    {
        await roundsRepository.CreateRoundAsync(round.ToEntity(), cancellationToken);

        // update player scores
        await Task.WhenAll(round.PlayerData
            .Where(pair => pair.Value.PointsAwarded > 0)
            .Select(pair => playerService.IncrementPlayerScoreAsync(pair.Key, pair.Value.PointsAwarded)));
    }

    private RoundSummary CreateRoundSummary(Round round) =>
        new()
        {
            Word = round.Word.Id,
            PartOfSpeech = round.WordHint.Definition.PartOfSpeech,
            EndReason = round.EndReason.GetValueOrDefault(),
            RoundId = round.Id,
            DefinitionId = round.WordHint.Definition.Id,
            NextRoundStart = DateTime.UtcNow + postRoundDelay,
            Scores = round.PlayerData
                .Where(pair => pair.Value.PointsAwarded > 0)
                .Select(pair => CreateScoreLine(pair.Key, pair.Value))
        };

    private ScoreLine CreateScoreLine(Guid playerId, RoundPlayerData data)
    {
        var player = playerState.GetPlayerById(playerId);

        if (player is null)
            logger.LogWarning("Player {PlayerId} not found when attempting to create score line", playerId);

        return new ScoreLine
        {
            PlayerName = player?.Name ?? string.Empty,
            ConnectionId = player?.ConnectionId ?? string.Empty,
            CountryName = player?.GeoLocation?.CountryName ?? string.Empty,
            CountryCode = player?.GeoLocation?.CountryCode ?? string.Empty,
            PointsAwarded = data.PointsAwarded,
            GuessCount = data.GuessCount,
            GuessTimeMilliseconds = data.GuessTime.TotalMilliseconds
        };
    }

    private static IEnumerable<int> GetShuffledRange(int maximum) =>
        Enumerable.Range(0, maximum).OrderBy(_ => Random.Shared.Next());

    private static LetterHint CreateLetterHint(Word word, int index) =>
        new(index + 1, word.Id[index]);

    private static Task PublishLetterHintAddedEventAsync(LetterHint letterHint, CancellationToken cancellationToken) =>
        new LetterHintAddedEvent(letterHint).PublishAsync(cancellation: cancellationToken);
}
