using FastEndpoints;
using OhMyWord.Core.Models;
using OhMyWord.Logic.Contracts.Events;
using OhMyWord.Logic.Services.State;

namespace OhMyWord.Logic.Contracts.Handlers;

public class PlayerDisconnectedHandler : IEventHandler<PlayerDisconnectedEvent>
{
    private readonly IPlayerState playerState;
    private readonly IRoundState roundState;

    public PlayerDisconnectedHandler(IPlayerState playerState, IRoundState roundState)
    {
        this.playerState = playerState;
        this.roundState = roundState;
    }

    public Task HandleAsync(PlayerDisconnectedEvent eventModel, CancellationToken cancellationToken = new())
    {
        playerState.RemovePlayer(eventModel.ConnectionId);

        // end round if player count is zero
        if (playerState.PlayerCount == 0)
            roundState.EndRound(RoundEndReason.NoPlayersLeft);

        return Task.CompletedTask;
    }
}
