using OhMyWord.Api.Services;
using OhMyWord.Domain.Services;

namespace OhMyWord.Api.Commands.RegisterPlayer;

public class RegisterPlayerHandler : ICommandHandler<RegisterPlayerCommand, RegisterPlayerResponse>
{
    private readonly IPlayerService playerService;
    private readonly IGameService gameService;

    public RegisterPlayerHandler(IPlayerService playerService, IGameService gameService)
    {
        this.playerService = playerService;
        this.gameService = gameService;
    }

    public async Task<RegisterPlayerResponse> ExecuteAsync(RegisterPlayerCommand command,
        CancellationToken cancellationToken)
    {
        var player = await playerService.AddPlayerAsync(command.VisitorId, command.ConnectionId, command.IpAddress,
            command.UserId);
        gameService.AddPlayer(player.Id);

        return new RegisterPlayerResponse
        {
            PlayerCount = playerService.PlayerCount, Score = player.Score, GameState = gameService.GameState
        };
    }
}
