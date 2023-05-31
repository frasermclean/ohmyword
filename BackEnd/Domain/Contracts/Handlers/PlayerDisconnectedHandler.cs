using FastEndpoints;
using OhMyWord.Domain.Contracts.Events;
using OhMyWord.Domain.Services.State;

namespace OhMyWord.Domain.Contracts.Handlers;

public class PlayerDisconnectedHandler : IEventHandler<PlayerDisconnectedEvent>
{
    private readonly IPlayerState playerState;

    public PlayerDisconnectedHandler(IPlayerState playerState)
    {
        this.playerState = playerState;
    }

    public Task HandleAsync(PlayerDisconnectedEvent eventModel, CancellationToken cancellationToken = new())
    {
        playerState.RemovePlayer(eventModel.ConnectionId);
        return Task.CompletedTask;
    }
}
