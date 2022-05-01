using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using OhMyWord.Api.Hubs;
using OhMyWord.Api.Responses.Game;
using OhMyWord.Services.Game;

namespace OhMyWord.Api;

public class GameCoordinator : BackgroundService
{
    private readonly IGameService gameService;

    public GameCoordinator(IGameService gameService, IHubContext<GameHub, IGameHub> gameHubContext, IMapper mapper)
    {
        this.gameService = gameService;

        gameService.RoundStarted += async (_, args) => await gameHubContext.Clients.All.SendRoundStarted(mapper.Map<RoundStartResponse>(args));
        gameService.RoundEnded += async (_, args) => await gameHubContext.Clients.All.SendRoundEnded(mapper.Map<RoundEndResponse>(args));
        gameService.LetterHintAdded += async letterHint => await gameHubContext.Clients.All.SendLetterHint(letterHint);
    }

    protected override Task ExecuteAsync(CancellationToken cancellationToken) =>
        gameService.ExecuteGameAsync(cancellationToken);
}
