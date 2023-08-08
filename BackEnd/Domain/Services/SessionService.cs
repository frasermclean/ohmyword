using Microsoft.Extensions.Logging;
using OhMyWord.Core.Models;
using OhMyWord.Domain.Extensions;
using OhMyWord.Domain.Services.State;
using OhMyWord.Integrations.Services.Repositories;

namespace OhMyWord.Domain.Services;

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
            var summary = await roundState.ExecuteRoundAsync(cancellationToken);

            // session ended due to all players leaving
            if (summary.EndReason == RoundEndReason.NoPlayersLeft)
                break;

            // post round delay
            var delay = summary.NextRoundStart - DateTime.UtcNow;
            await Task.Delay(delay, cancellationToken);
        }
    }

    public Task SaveSessionAsync(Session session, CancellationToken cancellationToken)
        => sessionsRepository.CreateSessionAsync(session.ToEntity(), cancellationToken);
}
