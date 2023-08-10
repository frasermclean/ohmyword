using FastEndpoints;
using OhMyWord.Domain.Contracts.Commands;
using OhMyWord.Domain.Models;
using OhMyWord.Domain.Services;
using OhMyWord.Domain.Services.State;

namespace OhMyWord.Domain.Contracts.Handlers;

public class RegisterPlayerHandler : ICommandHandler<RegisterPlayerCommand, RegisterPlayerResult>
{
    private readonly IPlayerService playerService;
    private readonly IPlayerState playerState;
    private readonly IRoundState roundState;

    public RegisterPlayerHandler(IPlayerService playerService, IPlayerState playerState, IRoundState roundState)
    {
        this.playerService = playerService;
        this.playerState = playerState;
        this.roundState = roundState;
    }

    public async Task<RegisterPlayerResult> ExecuteAsync(RegisterPlayerCommand command,
        CancellationToken cancellationToken = new())
    {
        // get or create player and add to player state
        var player = await playerService.GetPlayerAsync(command.PlayerId, command.VisitorId,
            command.ConnectionId, command.IpAddress, command.UserId, cancellationToken);
        var isSuccessful = playerState.AddPlayer(player);

        // add player to round if round is active
        if (isSuccessful && roundState.IsActive)
            roundState.AddPlayer(player.Id);

        return new RegisterPlayerResult
        {
            IsSuccessful = isSuccessful,
            PlayerId = player.Id,
            PlayerCount = playerState.PlayerCount,
            Score = player.Score,
            RegistrationCount = player.RegistrationCount,
            RoundStateSnapshot = roundState.GetSnapshot()
        };
    }
}
