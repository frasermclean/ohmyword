using FastEndpoints;
using OhMyWord.Api.Services;

namespace OhMyWord.Api.Events.VisitorDisconnected;

public class VisitorDisconnectedHandler : IEventHandler<VisitorDisconnectedEvent>
{
    private readonly ILogger<VisitorDisconnectedHandler> logger;
    private readonly IVisitorService visitorService;
    private readonly IGameService gameService;

    public VisitorDisconnectedHandler(ILogger<VisitorDisconnectedHandler> logger, IVisitorService visitorService,
        IGameService gameService)
    {
        this.logger = logger;
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
