using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using OhMyWord.Api.Hubs;
using OhMyWord.Api.Options;
using OhMyWord.Api.Responses.Game;

namespace OhMyWord.Api.Services;

public class GameCoordinator : BackgroundService
{
    private readonly ILogger<GameCoordinator> logger;
    private readonly IGameService gameService;
    private readonly IHubContext<GameHub, IGameHub> gameHubContext;

    private GameCoordinatorOptions Options { get; }
    private int RoundCount { get; set; }

    public GameCoordinator(
        ILogger<GameCoordinator> logger,
        IOptions<GameCoordinatorOptions> options,
        IGameService gameService,
        IHubContext<GameHub, IGameHub> gameHubContext)
    {
        this.logger = logger;
        this.gameService = gameService;
        this.gameHubContext = gameHubContext;
        Options = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var currentRoundExpiry = DateTime.UtcNow.AddSeconds(Options.RoundLength);
            var currentWord = await gameService.SelectNextWord(currentRoundExpiry);
            var roundDelay = TimeSpan.FromSeconds(Options.RoundLength);

            await gameHubContext.Clients.All.SendHint(gameService.CurrentHint);

            logger.LogDebug("Round: {count} has begun. Current word is \"{value}\". Round will end at: {expiry}", ++RoundCount, currentWord.Value, currentRoundExpiry);
            await Task.Delay(roundDelay, cancellationToken);
        }
    }
}
