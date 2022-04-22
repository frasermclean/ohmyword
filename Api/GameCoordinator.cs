using Microsoft.AspNetCore.SignalR;
using OhMyWord.Api.Hubs;
using OhMyWord.Services.Game;

namespace OhMyWord.Api;

public class GameCoordinator : BackgroundService
{
    private readonly IGameService gameService;

    public GameCoordinator(IGameService gameService, IHubContext<GameHub, IGameHub> gameHubContext)
    {
        this.gameService = gameService;

        gameService.RoundStarted += async roundStart => await gameHubContext.Clients.All.SendRoundStarted(roundStart);
        gameService.RoundEnded += async roundEnd => await gameHubContext.Clients.All.SendRoundEnded(roundEnd);
        gameService.LetterHintAdded += async letterHint => await gameHubContext.Clients.All.SendLetterHint(letterHint);
    }

    protected override Task ExecuteAsync(CancellationToken cancellationToken) =>
        gameService.ExecuteGameAsync(cancellationToken);
}
