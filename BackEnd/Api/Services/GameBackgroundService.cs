namespace OhMyWord.Api.Services;

public class GameBackgroundService : BackgroundService
{
    private readonly IGameService gameService;

    public GameBackgroundService(IGameService gameService)
    {
        this.gameService = gameService;
    }

    protected override Task ExecuteAsync(CancellationToken cancellationToken) =>
        gameService.ExecuteGameLoopAsync(cancellationToken);
}
