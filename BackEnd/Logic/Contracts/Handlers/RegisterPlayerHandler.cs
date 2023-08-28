using FastEndpoints;
using OhMyWord.Logic.Contracts.Commands;
using OhMyWord.Logic.Contracts.Parameters;
using OhMyWord.Logic.Models;
using OhMyWord.Logic.Services;
using OhMyWord.Logic.Services.State;

namespace OhMyWord.Logic.Contracts.Handlers;

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
        var parameters = new GetPlayerParameters(command.PlayerId, command.VisitorId, command.ConnectionId,
            command.IpAddress, command.UserId);
        var player = await playerService.GetPlayerAsync(parameters, cancellationToken);
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
