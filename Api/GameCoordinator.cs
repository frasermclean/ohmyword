using Microsoft.AspNetCore.SignalR;
using OhMyWord.Api.Hubs;
using OhMyWord.Core.Models;
using OhMyWord.Services.Game;

namespace OhMyWord.Api;

public class GameCoordinator : BackgroundService
{
    private readonly IGameService gameService;

    public GameCoordinator(
        IGameService gameService,
        IHubContext<GameHub, IGameHub> gameHubContext
        )
    {
        this.gameService = gameService;

        gameService.GameStatusChanged += async gameStatus => await gameHubContext.Clients.All.SendGameStatus(gameStatus);
        gameService.WordHintChanged += async wordHint => await gameHubContext.Clients.All.SendWordHint(wordHint);
        gameService.LetterHintAdded += async letterHint => await gameHubContext.Clients.All.SendLetterHint(letterHint);
    }

    protected override Task ExecuteAsync(CancellationToken cancellationToken) =>
        gameService.StartGameAsync(cancellationToken);
}
