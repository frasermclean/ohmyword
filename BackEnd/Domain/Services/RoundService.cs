using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OhMyWord.Domain.Contracts.Notifications;
using OhMyWord.Domain.Extensions;
using OhMyWord.Domain.Models;
using OhMyWord.Domain.Options;
using OhMyWord.Infrastructure.Models.Entities;
using OhMyWord.Infrastructure.Services;

namespace OhMyWord.Domain.Services;

public interface IRoundService
{
    Task<Round> CreateRoundAsync(int roundNumber, Guid sessionId, CancellationToken cancellationToken = default);
    Task ExecuteRoundAsync(Round round, CancellationToken cancellationToken = default);
    Task SaveRoundAsync(Round round, CancellationToken cancellationToken = default);
    Task<(TimeSpan, RoundSummary)> GetRoundEndDataAsync(Round round, CancellationToken cancellationToken = default);
}

public class RoundService : IRoundService
{
    private readonly ILogger<RoundService> logger;
    private readonly IPublisher publisher;
    private readonly IPlayerState playerState;
    private readonly IWordsService wordsService;
    private readonly IRoundsRepository roundsRepository;
    private readonly IGeoLocationService geoLocationService;

    private readonly TimeSpan letterHintDelay;
    private readonly TimeSpan postRoundDelay;
    private readonly int guessLimit;

    public RoundService(ILogger<RoundService> logger, IOptions<RoundOptions> options, IPublisher publisher,
        IPlayerState playerState, IWordsService wordsService, IRoundsRepository roundsRepository,
        IGeoLocationService geoLocationService)
    {
        this.logger = logger;
        this.publisher = publisher;
        this.playerState = playerState;
        this.wordsService = wordsService;
        this.roundsRepository = roundsRepository;
        this.geoLocationService = geoLocationService;

        letterHintDelay = TimeSpan.FromSeconds(options.Value.LetterHintDelay);
        postRoundDelay = TimeSpan.FromSeconds(options.Value.PostRoundDelay);
        guessLimit = options.Value.GuessLimit;
    }

    public async Task<Round> CreateRoundAsync(int roundNumber, Guid sessionId,
        CancellationToken cancellationToken = default)
    {
        var word = await wordsService.GetRandomWordAsync(cancellationToken);

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
    }

    public async Task<(TimeSpan, RoundSummary)> GetRoundEndDataAsync(Round round,
        CancellationToken cancellationToken = default)
    {
        var summary = new RoundSummary
        {
            Word = round.Word.Id,
            PartOfSpeech = round.WordHint.PartOfSpeech,
            EndReason = round.EndReason.GetValueOrDefault(),
            RoundId = round.Id,
            DefinitionId = round.WordHint.DefinitionId,
            NextRoundStart = DateTime.UtcNow + postRoundDelay,
            Scores = await round.GetPlayerData()
                .ToAsyncEnumerable()
                .Where(data => data.PointsAwarded > 0)
                .SelectAwait(async data => await CreateScoreLineAsync(data, cancellationToken))
                .ToListAsync(cancellationToken)
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
                await SendLetterHintAddedNotificationAsync(letterHint, cancellationToken);
            }
        }
        catch (TaskCanceledException)
        {
            logger.LogInformation("Round {RoundNumber} has been terminated early. Reason: {EndReason}",
                round.Number, round.EndReason);
        }
    }

    private Task SendLetterHintAddedNotificationAsync(LetterHint letterHint, CancellationToken cancellationToken)
    {
        var notification = new LetterHintAddedNotification(letterHint);
        return publisher.Publish(notification, cancellationToken);
    }

    private async Task<ScoreLine> CreateScoreLineAsync(RoundPlayerData data, CancellationToken cancellationToken)
    {
        var player = playerState.GetPlayerById(data.PlayerId);
        var countryCode = player is not null
            ? await geoLocationService.GetCountryCodeAsync(player.IpAddress, cancellationToken)
            : string.Empty;

        return new ScoreLine
        {
            PlayerName = player?.Name ?? string.Empty,
            ConnectionId = player?.ConnectionId ?? string.Empty,
            CountryCode = countryCode,
            PointsAwarded = data.PointsAwarded,
            GuessCount = data.GuessCount,
            GuessTimeMilliseconds = data.GuessTime.TotalMilliseconds
        };
    }

    private static IEnumerable<int> GetShuffledIndices(Word word) =>
        Enumerable.Range(0, word.Length).OrderBy(_ => Random.Shared.Next());
}
