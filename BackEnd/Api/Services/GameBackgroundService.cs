using OhMyWord.Domain.Services;

namespace OhMyWord.Api.Services;

public class GameBackgroundService : BackgroundService
{
    private readonly IStateManager stateManager;
    private readonly ISessionService sessionService;
    private readonly IPlayerService playerService;

    public GameBackgroundService(IStateManager stateManager, ISessionService sessionService,
        IPlayerService playerService)
    {
        this.stateManager = stateManager;
        this.sessionService = sessionService;
        this.playerService = playerService;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            // wait for players to join
            while (playerService.PlayerCount == 0)
            {
                await Task.Delay(1000, cancellationToken);
            }

            // start a new session
            using var session = stateManager.NextSession();
            await sessionService.ExecuteSessionAsync(session, cancellationToken);

            // reset the state manager
            stateManager.Reset();
        }
    }
}
