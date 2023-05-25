using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OhMyWord.Domain.Models;
using OhMyWord.Domain.Notifications;
using OhMyWord.Domain.Options;

namespace OhMyWord.Domain.Services;

public interface ISessionService
{
    Task ExecuteSessionAsync(Session session, CancellationToken cancellationToken = default);
}

public sealed class SessionService : ISessionService
{
    private readonly ILogger<SessionService> logger;
    private readonly IStateManager stateManager;
    private readonly IPublisher publisher;
    private readonly IRoundService roundService;

    private readonly TimeSpan postRoundDelay;

    public SessionService(ILogger<SessionService> logger, IOptions<RoundOptions> options, IStateManager stateManager,
        IPublisher publisher, IRoundService roundService)
    {
        this.logger = logger;
        this.stateManager = stateManager;
        this.publisher = publisher;
        this.roundService = roundService;

        postRoundDelay = TimeSpan.FromSeconds(options.Value.PostRoundDelay);
    }

    public async Task ExecuteSessionAsync(Session session, CancellationToken cancellationToken)
    {
        logger.LogInformation("Starting session: {SessionId}", session.Id);

        // create and execute rounds while there are players
        do
        {
            // load the next round
            using var round = await stateManager.NextRoundAsync(cancellationToken);
            await SendRoundStartedNotificationAsync(round, cancellationToken);

            // execute round            
            await roundService.ExecuteRoundAsync(round, cancellationToken);
            await roundService.SaveRoundAsync(round, cancellationToken);

            // post round delay
            await Task.WhenAll(
                SendRoundEndedNotificationAsync(round, cancellationToken),
                Task.Delay(postRoundDelay, cancellationToken));
        } while (stateManager.SessionState != SessionState.Waiting);
    }

    private Task SendRoundStartedNotificationAsync(Round round, CancellationToken cancellationToken)
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

    private Task SendRoundEndedNotificationAsync(Round round, CancellationToken cancellationToken)
    {
        var scores = round.GetPlayerData()
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
            });

        var notification = new RoundEndedNotification
        {
            Word = round.Word.Id,
            EndReason = round.EndReason ?? throw new InvalidOperationException("Round has not ended yet"),
            RoundId = round.Id,
            DefinitionId = round.WordHint.DefinitionId,
            NextRoundStart = DateTime.UtcNow + postRoundDelay,
            Scores = scores
        };

        return publisher.Publish(notification, cancellationToken);
    }
}
