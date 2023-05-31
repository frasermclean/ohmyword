using Microsoft.AspNetCore.SignalR;
using OhMyWord.Api.Hubs;
using OhMyWord.Domain.Contracts.Events;

namespace OhMyWord.Api.Handlers;

public class RoundEndedHandler : IEventHandler<RoundEndedEvent>
{
    private readonly IHubContext<GameHub, IGameHub> gameHubContext;

    public RoundEndedHandler(IHubContext<GameHub, IGameHub> gameHubContext)
    {
        this.gameHubContext = gameHubContext;
    }

    public Task HandleAsync(RoundEndedEvent eventModel, CancellationToken cancellationToken = new())
        => gameHubContext.Clients.All.SendRoundEnded(eventModel.Summary, cancellationToken);
}
