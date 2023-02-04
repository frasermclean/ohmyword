using FastEndpoints;
using OhMyWord.Api.Services;

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

    public async Task<RegisterVisitorResponse> ExecuteAsync(RegisterVisitorCommand command,
        CancellationToken cancellationToken)
    {
        var visitor = await visitorService.AddVisitorAsync(command.VisitorId, command.ConnectionId);
        gameService.AddVisitor(command.VisitorId);

        return new RegisterVisitorResponse
        {
            VisitorCount = visitorService.VisitorCount, Score = visitor.Score, GameState = gameService.State,
        };
    }
}
