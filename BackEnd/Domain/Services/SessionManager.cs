using Microsoft.Extensions.Logging;
using OhMyWord.Domain.Models;

namespace OhMyWord.Domain.Services;

public interface ISessionManager
{
    /// <summary>
    /// Current player count.
    /// </summary>
    int PlayerCount { get; }

    /// <summary>
    /// Starts and executes new session.
    /// </summary>
    Task ExecuteSessionAsync(CancellationToken cancellationToken = default);
}

public sealed class SessionManager : ISessionManager
{
    private readonly ILogger<SessionManager> logger;
    private readonly IRoundService roundService;

    private Session session = Session.Default;


    public SessionManager(ILogger<SessionManager> logger, IRoundService roundService)
    {
        this.logger = logger;
        this.roundService = roundService;
    }

    public int PlayerCount { get; set; }

    public async Task ExecuteSessionAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            session = new Session();
            logger.LogInformation("Starting new session {SessionId}", session.Id);
            await Task.Delay(1000, cancellationToken);
        }
    }
}
