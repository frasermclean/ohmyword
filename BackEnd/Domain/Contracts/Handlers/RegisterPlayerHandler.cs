using FastEndpoints;
using OhMyWord.Domain.Contracts.Commands;
using OhMyWord.Domain.Contracts.Results;
using OhMyWord.Domain.Services;

namespace OhMyWord.Domain.Contracts.Handlers;

public class RegisterPlayerHandler : ICommandHandler<RegisterPlayerCommand, RegisterPlayerResult>
{
    private readonly IPlayerService playerService;
    private readonly IStateManager stateManager;

    public RegisterPlayerHandler(IPlayerService playerService, IStateManager stateManager)
    {
        this.playerService = playerService;
        this.stateManager = stateManager;
    }

    public async Task<RegisterPlayerResult> ExecuteAsync(RegisterPlayerCommand command,
        CancellationToken cancellationToken = new())
    {
        var player = await playerService.GetPlayerAsync(command.PlayerId, command.VisitorId,
            command.ConnectionId, command.IpAddress, command.UserId, cancellationToken);
        var isSuccessful = stateManager.PlayerState.AddPlayer(player);

        return new RegisterPlayerResult
        {
            IsSuccessful = isSuccessful,
            PlayerId = player.Id,
            PlayerCount = stateManager.PlayerState.PlayerCount,
            Score = player.Score,
            RegistrationCount = player.RegistrationCount,
            StateSnapshot = stateManager.GetStateSnapshot()
        };
    }
}
