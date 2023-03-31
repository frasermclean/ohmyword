using OhMyWord.Api.Services;
using OhMyWord.Domain.Services;

namespace OhMyWord.Api.Events.PlayerDisconnected;

public class PlayerDisconnectedHandler : IEventHandler<PlayerDisconnectedEvent>
{
    private readonly IPlayerService playerService;
    private readonly IGameService gameService;

    public PlayerDisconnectedHandler(IPlayerService playerService, IGameService gameService)
    {
        this.playerService = playerService;
        this.gameService = gameService;
    }

    public Task HandleAsync(PlayerDisconnectedEvent disconnectedEvent, CancellationToken cancellationToken)
    {
        playerService.RemovePlayer(disconnectedEvent.ConnectionId);
        gameService.RemovePlayer(disconnectedEvent.ConnectionId);
        return Task.CompletedTask;
    }
}
