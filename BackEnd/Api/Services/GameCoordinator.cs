namespace OhMyWord.Api.Services;

public class GameCoordinator : BackgroundService
{
    private readonly IGameService gameService;

    public GameCoordinator(IGameService gameService)
    {
        this.gameService = gameService;
    }

    protected override Task ExecuteAsync(CancellationToken cancellationToken) =>
        gameService.ExecuteGameAsync(cancellationToken);
}
