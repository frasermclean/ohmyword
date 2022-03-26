namespace OhMyWord.Api.Services;

public class GameCoordinatorService : BackgroundService
{
    private readonly IGameService gameService;

    public GameCoordinatorService(IGameService gameService)
    {
        this.gameService = gameService;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var response = await gameService.SelectNextWord();
            var delay = response.Expiry - DateTime.UtcNow;
            await Task.Delay(delay, cancellationToken);
        }
    }
}
