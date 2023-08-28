using FastEndpoints;
using Microsoft.Extensions.Logging;
using OhMyWord.Core.Models;
using OhMyWord.Logic.Contracts.Events;
using OhMyWord.Logic.Extensions;
using OhMyWord.Logic.Models;
using OhMyWord.Logic.Services.State;

namespace OhMyWord.Logic.Services;

public interface ISessionService
{
    Task ExecuteSessionAsync(Session session, CancellationToken cancellationToken = default);
    Task SaveSessionAsync(Session session, CancellationToken cancellationToken = default);
}

public sealed class SessionService : ISessionService
{
    private readonly ILogger<SessionService> logger;
    private readonly IPlayerState playerState;
    private readonly IRoundState roundState;
    private readonly ISessionsRepository sessionsRepository;

    public SessionService(ILogger<SessionService> logger, IPlayerState playerState, IRoundState roundState,
        ISessionsRepository sessionsRepository)
    {
        this.logger = logger;
        this.playerState = playerState;
        this.roundState = roundState;
        this.sessionsRepository = sessionsRepository;
    }

    public async Task ExecuteSessionAsync(Session session, CancellationToken cancellationToken)
    {
        logger.LogInformation("Starting session: {SessionId}", session.Id);

        // create and execute rounds while there are players
        while (playerState.PlayerCount > 0)
        {
            // start and execute round
            var startData = await roundState.CreateRoundAsync(cancellationToken);
            await SendRoundStartedNotificationAsync(startData, cancellationToken);

            // execute round
            var summary = await roundState.ExecuteRoundAsync(cancellationToken);

            // session ended due to all players leaving
            if (summary.EndReason == RoundEndReason.NoPlayersLeft)
                break;

            await SendRoundEndedNotificationAsync(summary, cancellationToken);

            // post round delay
            var delay = summary.NextRoundStart - DateTime.UtcNow;
            await Task.Delay(delay, cancellationToken);
        }
    }

    public Task SaveSessionAsync(Session session, CancellationToken cancellationToken)
        => sessionsRepository.CreateSessionAsync(session.ToEntity(), cancellationToken);

    private static Task SendRoundStartedNotificationAsync(RoundStartData data, CancellationToken cancellationToken)
        => new RoundStartedEvent(data).PublishAsync(cancellation: cancellationToken);

    private static Task SendRoundEndedNotificationAsync(RoundSummary summary, CancellationToken cancellationToken)
        => new RoundEndedEvent(summary).PublishAsync(cancellation: cancellationToken);
}
