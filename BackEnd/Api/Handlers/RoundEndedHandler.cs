using MediatR;
using Microsoft.AspNetCore.SignalR;
using OhMyWord.Api.Hubs;
using OhMyWord.Domain.Contracts.Notifications;

namespace OhMyWord.Api.Handlers;

public class RoundEndedHandler : INotificationHandler<RoundEndedNotification>
{
    private readonly IHubContext<GameHub, IGameHub> gameHubContext;

    public RoundEndedHandler(IHubContext<GameHub, IGameHub> gameHubContext)
    {
        this.gameHubContext = gameHubContext;
    }
    
    public Task Handle(RoundEndedNotification notification, CancellationToken cancellationToken)
    {
        return gameHubContext.Clients.All.SendRoundEnded(notification, cancellationToken);
    }
}
