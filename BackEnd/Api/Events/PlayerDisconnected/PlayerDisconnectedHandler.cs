using OhMyWord.Domain.Services;

namespace OhMyWord.Api.Events.PlayerDisconnected;

public class PlayerDisconnectedHandler : IEventHandler<PlayerDisconnectedEvent>
{
    private readonly IPlayerService playerService;

    public PlayerDisconnectedHandler(IPlayerService playerService)
    {
        this.playerService = playerService;
    }

    public Task HandleAsync(PlayerDisconnectedEvent disconnectedEvent, CancellationToken cancellationToken)
    {
        playerService.RemovePlayer(disconnectedEvent.ConnectionId);
        return Task.CompletedTask;
    }
}
