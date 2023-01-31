using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using OhMyWord.Api.Hubs;
using OhMyWord.Core.Responses.Game;

namespace OhMyWord.Api.Services;

public class GameCoordinator : BackgroundService
{
    private readonly IGameService gameService;

    public GameCoordinator(IGameService gameService, IHubContext<GameHub, IGameHub> gameHubContext, IMapper mapper)
    {
        this.gameService = gameService;
    }

    protected override Task ExecuteAsync(CancellationToken cancellationToken) =>
        gameService.ExecuteGameAsync(cancellationToken);
}
