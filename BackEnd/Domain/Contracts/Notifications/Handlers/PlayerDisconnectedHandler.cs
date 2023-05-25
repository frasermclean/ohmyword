using MediatR;
using OhMyWord.Domain.Services;

namespace OhMyWord.Domain.Contracts.Notifications.Handlers;

public class PlayerDisconnectedHandler : INotificationHandler<PlayerDisconnectedNotification>
{
    private readonly IPlayerState playerState;

    public PlayerDisconnectedHandler(IPlayerState playerState)
    {
        this.playerState = playerState;
    }

    public Task Handle(PlayerDisconnectedNotification notification, CancellationToken cancellationToken)
    {
        playerState.RemovePlayer(notification.ConnectionId);

        return Task.CompletedTask;
    }
}
