using OhMyWord.Domain.Services;

namespace OhMyWord.Api.Services;

public class GameBackgroundService : BackgroundService
{
    private readonly ISessionManager sessionManager;

    public GameBackgroundService(ISessionManager sessionManager)
    {
        this.sessionManager = sessionManager;
    }

    protected override Task ExecuteAsync(CancellationToken cancellationToken) =>
        sessionManager.ExecuteAsync(cancellationToken);
}
