using MediatR;
using Microsoft.AspNetCore.SignalR;
using OhMyWord.Api.Hubs;
using OhMyWord.Domain.Notifications;

namespace OhMyWord.Api.Handlers;

public class RoundStartedHandler : INotificationHandler<RoundStartedNotification>
{
    private readonly IHubContext<GameHub, IGameHub> gameHubContext;

    public RoundStartedHandler(IHubContext<GameHub, IGameHub> gameHubContext)
    {
        this.gameHubContext = gameHubContext;
    }

    public Task Handle(RoundStartedNotification notification, CancellationToken cancellationToken)
    {
        return gameHubContext.Clients.All.SendRoundStarted(notification, cancellationToken);
    }
}
