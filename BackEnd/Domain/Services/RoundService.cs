using FastEndpoints;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OhMyWord.Core.Models;
using OhMyWord.Domain.Contracts.Events;
using OhMyWord.Domain.Extensions;
using OhMyWord.Domain.Options;
using OhMyWord.Domain.Services.State;
using OhMyWord.Infrastructure.Services.Repositories;

namespace OhMyWord.Domain.Services;

public interface IRoundService
{
    Task<Round> CreateRoundAsync(int roundNumber, Guid sessionId, CancellationToken cancellationToken = default);
    Task ExecuteRoundAsync(Round round, CancellationToken cancellationToken = default);
    Task SaveRoundAsync(Round round, CancellationToken cancellationToken = default);
    (TimeSpan, RoundSummary) GetRoundEndData(Round round);
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

    public async Task<Round> CreateRoundAsync(int roundNumber, Guid sessionId,
        CancellationToken cancellationToken = default)
    {
        var word = await wordQueueService.GetNextWordAsync(cancellationToken: cancellationToken);

        return new Round(word, letterHintDelay, playerState.PlayerIds)
        {
            Number = roundNumber, GuessLimit = guessLimit, SessionId = sessionId,
        };
    }

    public async Task ExecuteRoundAsync(Round round, CancellationToken cancellationToken)
    {
        logger.LogInformation("Starting round {RoundNumber} for session {SessionId}", round.Number, round.SessionId);

        // create linked cancellation token source
        using var linkedTokenSource =
            CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, round.CancellationToken);

        // send letter hints
        await SendLetterHintsAsync(round, linkedTokenSource.Token);

        // round ended due to timeout
        if (round.EndReason is null)
            round.EndRound(RoundEndReason.Timeout);
    }

    public async Task SaveRoundAsync(Round round, CancellationToken cancellationToken)
    {
        await roundsRepository.CreateRoundAsync(round.ToEntity(), cancellationToken);

        // update player scores
        await Task.WhenAll(round.GetPlayerData()
            .Where(data => data.PointsAwarded > 0)
            .Select(data => playerService.IncrementPlayerScoreAsync(data.PlayerId, data.PointsAwarded)));
    }

    public (TimeSpan, RoundSummary) GetRoundEndData(Round round)
    {
        var summary = new RoundSummary
        {
            Word = round.Word.Id,
            PartOfSpeech = round.WordHint.PartOfSpeech,
            EndReason = round.EndReason.GetValueOrDefault(),
            RoundId = round.Id,
            DefinitionId = round.WordHint.DefinitionId,
            NextRoundStart = DateTime.UtcNow + postRoundDelay,
            Scores = round.GetPlayerData()
                .Where(data => data.PointsAwarded > 0)
                .Select(CreateScoreLine)
        };

        return (postRoundDelay, summary);
    }

    private async Task SendLetterHintsAsync(Round round, CancellationToken cancellationToken)
    {
        try
        {
            foreach (var index in GetShuffledIndices(round.Word))
            {
                await Task.Delay(letterHintDelay, cancellationToken);

                var letterHint = round.Word.GetLetterHint(index + 1);
                round.WordHint.AddLetterHint(letterHint);
                await new LetterHintAddedEvent(letterHint).PublishAsync(cancellation: cancellationToken);
            }
        }
        catch (TaskCanceledException)
        {
            logger.LogInformation("Round {RoundNumber} has been terminated early. Reason: {EndReason}",
                round.Number, round.EndReason);
        }
    }

    private ScoreLine CreateScoreLine(RoundPlayerData data)
    {
        var player = playerState.GetPlayerById(data.PlayerId);

        return new ScoreLine
        {
            PlayerName = player?.Name ?? string.Empty,
            ConnectionId = player?.ConnectionId ?? string.Empty,
            CountryName = player?.GeoLocation.CountryName ?? string.Empty,
            CountryCode = player?.GeoLocation.CountryCode ?? string.Empty,
            PointsAwarded = data.PointsAwarded,
            GuessCount = data.GuessCount,
            GuessTimeMilliseconds = data.GuessTime.TotalMilliseconds
        };
    }

    private static IEnumerable<int> GetShuffledIndices(Word word) =>
        Enumerable.Range(0, word.Length).OrderBy(_ => Random.Shared.Next());
}
