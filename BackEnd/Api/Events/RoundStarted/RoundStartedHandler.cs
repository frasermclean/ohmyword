using Microsoft.AspNetCore.SignalR;
using OhMyWord.Api.Hubs;

namespace OhMyWord.Api.Events.RoundStarted;

public class RoundStartedHandler : IEventHandler<RoundStartedEvent>
{
    private readonly IHubContext<GameHub, IGameHub> gameHubContext;

    public RoundStartedHandler(IHubContext<GameHub, IGameHub> gameHubContext)
    {
        this.gameHubContext = gameHubContext;
    }

    public Task HandleAsync(RoundStartedEvent roundStarted, CancellationToken cancellationToken)
        => gameHubContext.Clients.All.SendRoundStarted(roundStarted.Data, cancellationToken);
}
