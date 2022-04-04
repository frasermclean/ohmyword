using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using OhMyWord.Api.Hubs;
using OhMyWord.Api.Options;
using OhMyWord.Api.Responses.Game;
using OhMyWord.Services.Game;

namespace OhMyWord.Api;

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
            var roundDelay = TimeSpan.FromSeconds(Options.RoundLength);

            try
            {
                var currentWord = await gameService.SelectNextWord(currentRoundExpiry);
                await gameHubContext.Clients.All.SendHint(gameService.CurrentHint);

                logger.LogDebug("Round: {count} has begun. Current word is \"{currentWord}\". Round will end at: {expiry}", ++RoundCount, currentWord, currentRoundExpiry);
                await Task.Delay(roundDelay, cancellationToken);

                // end of round delay
                var nextRoundStart = DateTime.UtcNow.AddSeconds(Options.NextRoundDelay);
                await gameHubContext.Clients.All.SendRoundOver(new RoundOverResponse
                {
                    NextRoundStart = nextRoundStart,
                });
                await Task.Delay(nextRoundStart - DateTime.UtcNow, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Exception occurred in game coordinator main loop. Round count: {RoundCount}", RoundCount);
                await Task.Delay(roundDelay, cancellationToken);
            }
        }
    }
}
