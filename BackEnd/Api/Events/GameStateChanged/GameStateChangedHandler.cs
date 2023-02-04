using FastEndpoints;
using Microsoft.AspNetCore.SignalR;
using OhMyWord.Api.Hubs;

namespace OhMyWord.Api.Events.GameStateChanged;

public class GameStateChangedHandler : IEventHandler<GameStateChangedEvent>
{
    private readonly IHubContext<GameHub, IGameHub> gameHubContext;

    public GameStateChangedHandler(IHubContext<GameHub, IGameHub> gameHubContext)
    {
        this.gameHubContext = gameHubContext;
    }

    public Task HandleAsync(GameStateChangedEvent eventModel, CancellationToken cancellationToken)
        => gameHubContext.Clients.All.SendGameState(eventModel.GameState);
}
