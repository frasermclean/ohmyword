using FastEndpoints;
using OhMyWord.Domain.Contracts.Commands;
using OhMyWord.Domain.Contracts.Results;
using OhMyWord.Domain.Services;
using OhMyWord.Domain.Services.State;

namespace OhMyWord.Domain.Contracts.Handlers;

public class RegisterPlayerHandler : ICommandHandler<RegisterPlayerCommand, RegisterPlayerResult>
{
    private readonly IPlayerService playerService;
    private readonly IRootState rootState;

    public RegisterPlayerHandler(IPlayerService playerService, IRootState rootState)
    {
        this.playerService = playerService;
        this.rootState = rootState;
    }

    public async Task<RegisterPlayerResult> ExecuteAsync(RegisterPlayerCommand command,
        CancellationToken cancellationToken = new())
    {
        var player = await playerService.GetPlayerAsync(command.PlayerId, command.VisitorId,
            command.ConnectionId, command.IpAddress, command.UserId, cancellationToken);
        var isSuccessful = rootState.PlayerState.AddPlayer(player);

        return new RegisterPlayerResult
        {
            IsSuccessful = isSuccessful,
            PlayerId = player.Id,
            PlayerCount = rootState.PlayerState.PlayerCount,
            Score = player.Score,
            RegistrationCount = player.RegistrationCount,
            StateSnapshot = rootState.GetStateSnapshot()
        };
    }
}
