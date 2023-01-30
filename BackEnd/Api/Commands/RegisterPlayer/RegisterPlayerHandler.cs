using FastEndpoints;
using OhMyWord.Core.Responses.Game;
using OhMyWord.Core.Services;

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

    public async Task<RegisterPlayerResponse> ExecuteAsync(RegisterPlayerCommand command, CancellationToken cancellationToken)
    {
        var player = await playerService.AddPlayerAsync(command.VisitorId, command.ConnectionId);

        return new RegisterPlayerResponse
        {
            RoundActive = gameService.RoundActive,
            PlayerCount = playerService.PlayerCount,
            Score = player.Score,
            RoundStart = gameService.RoundActive
                ? new RoundStartResponse
                {
                    RoundNumber = gameService.Round.Number,
                    RoundId = gameService.Round.Id,
                    RoundStarted = gameService.Round.StartTime,
                    RoundEnds = gameService.Round.EndTime,
                    WordHint = gameService.Round.WordHint,
                }
                : null,
            RoundEnd = !gameService.RoundActive
                ? new RoundEndResponse
                {
                    RoundNumber = gameService.Round.Number,
                    RoundId = gameService.Round.Id,
                    WordId = gameService.Round.Word.Id,
                    EndReason = gameService.Round.EndReason,
                    NextRoundStart = gameService.Expiry,
                }
                : null,
        };
    }
}
