using Microsoft.Extensions.Logging;
using OhMyWord.Domain.Models;

namespace OhMyWord.Domain.Services;

public interface ISessionManager
{
    bool IsActive { get; }

    /// <summary>
    /// Starts and executes new session.
    /// </summary>
    Task ExecuteAsync(CancellationToken cancellationToken = default);
}

public sealed class SessionManager : ISessionManager
{
    private readonly ILogger<SessionManager> logger;
    private readonly IRoundManager roundManager;
    private readonly IPlayerService playerService;

    private Session session = Session.Default;

    public SessionManager(ILogger<SessionManager> logger, IRoundManager roundManager, IPlayerService playerService)
    {
        this.logger = logger;
        this.roundManager = roundManager;
        this.playerService = playerService;
    }

    public bool IsActive => session != Session.Default;

    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            // wait for players to join
            if (playerService.PlayerCount == 0)
            {
                await Task.Delay(1000, cancellationToken);
                continue;
            }

            // create new session
            session = new Session();
            logger.LogInformation("Starting new session {SessionId}", session.Id);

            // create and execute rounds while there are players
            do
            {
                await roundManager.CreateRoundAsync(session.Id, ++session.RoundCount, cancellationToken);
                await roundManager.ExecuteRoundAsync(cancellationToken);
            } while (roundManager.IsActive);

            logger.LogInformation("Ending session {SessionId}", session.Id);
            session = Session.Default;
        }
    }
}
