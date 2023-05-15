using Microsoft.AspNetCore.SignalR;
using OhMyWord.Api.Hubs;

namespace OhMyWord.Api.Events.RoundEnded;

public class RoundEndedHandler : IEventHandler<RoundEndedEvent>
{
    private readonly IHubContext<GameHub, IGameHub> gameHubContext;
    
    public RoundEndedHandler(IHubContext<GameHub, IGameHub> gameHubContext)
    {
        this.gameHubContext = gameHubContext;
    }
    
    public Task HandleAsync(RoundEndedEvent eventModel, CancellationToken cancellationToken)
    {
        return gameHubContext.Clients.All.SendRoundEnded(eventModel, cancellationToken);
    }
}
