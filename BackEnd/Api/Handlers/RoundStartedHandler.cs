using Microsoft.AspNetCore.SignalR;
using OhMyWord.Api.Hubs;
using OhMyWord.Api.Models;
using OhMyWord.Domain.Contracts.Events;

namespace OhMyWord.Api.Handlers;

public class RoundStartedHandler : IEventHandler<RoundStartedEvent>
{
    private readonly IHubContext<GameHub, IGameHub> gameHubContext;

    public RoundStartedHandler(IHubContext<GameHub, IGameHub> gameHubContext)
    {
        this.gameHubContext = gameHubContext;
    }

    public async Task HandleAsync(RoundStartedEvent eventModel, CancellationToken cancellationToken)
    {
        var response = RoundStartedResponse.FromRoundStartData(eventModel.Data);
        await gameHubContext.Clients.All.SendRoundStarted(response, cancellationToken);
    }
}
