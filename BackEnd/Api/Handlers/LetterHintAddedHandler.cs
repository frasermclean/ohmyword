using MediatR;
using Microsoft.AspNetCore.SignalR;
using OhMyWord.Api.Hubs;
using OhMyWord.Domain.Models.Notifications;

namespace OhMyWord.Api.Handlers;

public class LetterHintAddedHandler : INotificationHandler<LetterHintAddedNotification>
{
    private readonly IHubContext<GameHub, IGameHub> gameHubContext;

    public LetterHintAddedHandler(IHubContext<GameHub, IGameHub> gameHubContext)
    {
        this.gameHubContext = gameHubContext;
    }

    public Task Handle(LetterHintAddedNotification notification, CancellationToken cancellationToken)
        => gameHubContext.Clients.All.SendLetterHint(notification.LetterHint);
}
