using Microsoft.AspNetCore.SignalR;
using OhMyWord.Api.Hubs;
using OhMyWord.Domain.Contracts.Events;

namespace OhMyWord.Api.Handlers;

public class RoundStartedHandler : IEventHandler<RoundStartedEvent>
{
    private readonly IHubContext<GameHub, IGameHub> gameHubContext;

    public RoundStartedHandler(IHubContext<GameHub, IGameHub> gameHubContext)
    {
        this.gameHubContext = gameHubContext;
    }

    public Task HandleAsync(RoundStartedEvent eventModel, CancellationToken cancellationToken = new())
        => gameHubContext.Clients.All.SendRoundStarted(eventModel.Data, cancellationToken);
}
