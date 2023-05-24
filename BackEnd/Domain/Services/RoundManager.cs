using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OhMyWord.Domain.Extensions;
using OhMyWord.Domain.Models;
using OhMyWord.Domain.Models.Notifications;
using OhMyWord.Domain.Options;
using OhMyWord.Infrastructure.Services;

namespace OhMyWord.Domain.Services;

public interface IRoundManager
{
    bool IsActive { get; }
    
    Task CreateRoundAsync(Guid sessionId, int roundNumber, CancellationToken cancellationToken = default);
    Task ExecuteRoundAsync(CancellationToken cancellationToken = default);
    Task SaveRoundAsync(CancellationToken cancellationToken = default);
}

public class RoundManager : IRoundManager
{
    private readonly ILogger<RoundManager> logger;
    private readonly IPublisher publisher;
    private readonly IWordsService wordsService;
    private readonly IPlayerService playerService;
    private readonly IRoundsRepository roundsRepository;
    private readonly RoundOptions options;

    private Round round = Round.Default;

    public RoundManager(ILogger<RoundManager> logger, IPublisher publisher, IOptions<RoundOptions> options,
        IWordsService wordsService, IPlayerService playerService, IRoundsRepository roundsRepository)
    {
        this.logger = logger;
        this.publisher = publisher;
        this.wordsService = wordsService;
        this.playerService = playerService;
        this.roundsRepository = roundsRepository;
        this.options = options.Value;
    }

    public bool IsActive => playerService.PlayerCount > 0;

    public async Task CreateRoundAsync(Guid sessionId, int roundNumber, CancellationToken cancellationToken)
    {
        var word = await wordsService.GetRandomWordAsync(cancellationToken);

        logger.LogInformation("Creating round {RoundNumber} with word {Word}", roundNumber, word);

        round = new Round(word, options.LetterHintDelay, playerService.PlayerIds)
        {
            Number = roundNumber, GuessLimit = options.GuessLimit, SessionId = sessionId,
        };
    }

    public async Task ExecuteRoundAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Starting round {RoundNumber} for session {SessionId}", round.Number, round.SessionId);
        await SendRoundStartedNotificationAsync(cancellationToken);

        try
        {
            await SendLetterHintsAsync(round.CancellationToken);
        }
        catch (TaskCanceledException)
        {
            logger.LogInformation("Round {RoundNumber} has been terminated early. Reason: {EndReason}",
                round.Number, round.EndReason);
        }

        await SendRoundEndedNotificationAsync(cancellationToken);
    }

    public async Task SaveRoundAsync(CancellationToken cancellationToken)
    {
        await roundsRepository.CreateRoundAsync(round.ToEntity(), cancellationToken);
    }

    private async Task SendLetterHintsAsync(CancellationToken cancellationToken)
    {
        var previousIndices = new List<int>();

        while (previousIndices.Count < round.Word.Length && !cancellationToken.IsCancellationRequested)
        {
            var delay = (round.EndDate - round.StartDate) / round.Word.Length;
            await Task.Delay(delay, round.CancellationToken);

            int index;
            do index = Random.Shared.Next(round.Word.Length);
            while (previousIndices.Contains(index));
            previousIndices.Add(index);

            var letterHint = round.Word.GetLetterHint(index + 1);
            round.WordHint.AddLetterHint(letterHint);
            await SendLetterHintAddedNotificationAsync(letterHint, cancellationToken);
        }
    }

    private Task SendRoundStartedNotificationAsync(CancellationToken cancellationToken)
    {
        var notification = new RoundStartedNotification
        {
            RoundNumber = round.Number,
            RoundId = round.Id,
            WordHint = round.WordHint,
            StartDate = round.StartDate,
            EndDate = round.EndDate
        };

        return publisher.Publish(notification, cancellationToken);
    }

    private Task SendLetterHintAddedNotificationAsync(LetterHint letterHint, CancellationToken cancellationToken)
    {
        var notification = new LetterHintAddedNotification(letterHint);
        return publisher.Publish(notification, cancellationToken);
    }

    private Task SendRoundEndedNotificationAsync(CancellationToken cancellationToken)
    {
        var notification = new RoundEndedNotification
        {
            Word = round.Word.Id,
            EndReason = round.EndReason,
            RoundId = round.Id,
            DefinitionId = round.WordHint.DefinitionId,
            NextRoundStart = DateTime.UtcNow + TimeSpan.FromSeconds(options.PostRoundDelay),
            Scores = round.GetPlayerData()
                .Where(data => data.PointsAwarded > 0)
                .Select(data =>
                {
                    var player = null as Player; //playerService.GetPlayerById(data.PlayerId);
                    return new ScoreLine
                    {
                        PlayerName = string.Empty, // TODO: Calculate player name
                        ConnectionId = player?.ConnectionId ?? string.Empty,
                        CountryCode = string.Empty, // TODO: Calculate country code
                        PointsAwarded = data.PointsAwarded,
                        GuessCount = data.GuessCount,
                        GuessTimeMilliseconds = data.GuessTime.TotalMilliseconds
                    };
                })
        };

        return publisher.Publish(notification, cancellationToken);
    }
}
