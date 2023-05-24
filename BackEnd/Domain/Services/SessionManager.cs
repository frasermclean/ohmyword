using Microsoft.Extensions.Logging;
using OhMyWord.Domain.Models;

namespace OhMyWord.Domain.Services;

public interface ISessionManager
{
    /// <summary>
    /// Starts and executes new session.
    /// </summary>
    Task ExecuteAsync(CancellationToken cancellationToken = default);
}

public sealed class SessionManager : ISessionManager
{
    private readonly ILogger<SessionManager> logger;
    private readonly IStateProvider state;
    private readonly IRoundManager roundManager;
    private readonly IPlayerService playerService;

    public SessionManager(ILogger<SessionManager> logger, IStateProvider state, IRoundManager roundManager,
        IPlayerService playerService)
    {
        this.logger = logger;
        this.state = state;
        this.roundManager = roundManager;
        this.playerService = playerService;
    }

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
            var session = state.StartSession();
            logger.LogInformation("Starting new session {SessionId}", state.Session.Id);

            // create and execute rounds while there are players
            do await ExecuteRoundAsync(session, cancellationToken);
            while (playerService.PlayerCount > 0);

            logger.LogInformation("Ending session {SessionId}", state.Session.Id);
            state.Session.EndDate = DateTime.UtcNow;

            state.Reset();
        }
    }

    private async Task ExecuteRoundAsync(Session session, CancellationToken cancellationToken)
    {
        var roundNumber = state.IncrementRoundCount();
        using (state.Round = await roundManager.CreateRoundAsync(session.Id, roundNumber, cancellationToken))
        {
            await roundManager.ExecuteRoundAsync(state.Round, cancellationToken);
            await roundManager.SaveRoundAsync(state.Round, cancellationToken);
        }
    }
}
