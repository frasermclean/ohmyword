using FastEndpoints;
using OhMyWord.Api.Services;
using OhMyWord.Core.Responses.Game;

namespace OhMyWord.Api.Commands.RegisterVisitor;

public class RegisterVisitorHandler : ICommandHandler<RegisterVisitorCommand, RegisterVisitorResponse>
{
    private readonly IVisitorService visitorService;
    private readonly IGameService gameService;

    public RegisterVisitorHandler(IVisitorService visitorService, IGameService gameService)
    {
        this.visitorService = visitorService;
        this.gameService = gameService;
    }

    public async Task<RegisterVisitorResponse> ExecuteAsync(RegisterVisitorCommand command, CancellationToken cancellationToken)
    {
        var visitor = await visitorService.AddVisitorAsync(command.VisitorId, command.ConnectionId);
        gameService.AddVisitor(command.VisitorId);

        return new RegisterVisitorResponse
        {
            RoundActive = gameService.RoundActive,
            VisitorCount = visitorService.VisitorCount,
            Score = visitor.Score,
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
