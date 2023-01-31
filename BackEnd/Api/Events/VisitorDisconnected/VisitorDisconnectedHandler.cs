using FastEndpoints;
using OhMyWord.Api.Services;

namespace OhMyWord.Api.Events.VisitorDisconnected;

public class VisitorDisconnectedHandler : IEventHandler<VisitorDisconnectedEvent>
{
    private readonly IVisitorService visitorService;
    private readonly IGameService gameService;

    public VisitorDisconnectedHandler(IVisitorService visitorService, IGameService gameService)
    {
        this.visitorService = visitorService;
        this.gameService = gameService;
    }

    public Task HandleAsync(VisitorDisconnectedEvent disconnectedEvent, CancellationToken cancellationToken)
    {
        visitorService.RemoveVisitor(disconnectedEvent.ConnectionId);
        gameService.RemoveVisitor(disconnectedEvent.ConnectionId);
        return Task.CompletedTask;
    }
}
