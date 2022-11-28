using MediatR;
using OhMyWord.Core.Requests.Game;
using OhMyWord.Core.Responses.Game;
using OhMyWord.Core.Services;

namespace OhMyWord.Core.Handlers.Game;

public class RegisterPlayerHandler : IRequestHandler<RegisterPlayerRequest, RegisterPlayerResponse>
{
    private readonly IPlayerService playerService;
    private readonly IGameService gameService;

    public RegisterPlayerHandler(IPlayerService playerService, IGameService gameService)
    {
        this.playerService = playerService;
        this.gameService = gameService;
    }

    public async Task<RegisterPlayerResponse> Handle(RegisterPlayerRequest request, CancellationToken cancellationToken)
    {
        var player = await playerService.AddPlayerAsync(request.VisitorId, request.ConnectionId);

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
                    Word = gameService.Round.Word.Value,
                    EndReason = gameService.Round.EndReason,
                    NextRoundStart = gameService.Expiry,
                }
                : null,
        };
    }
}