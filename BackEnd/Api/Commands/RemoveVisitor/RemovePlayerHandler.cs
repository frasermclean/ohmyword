using FastEndpoints;
using OhMyWord.Core.Services;

namespace OhMyWord.Api.Commands.RemoveVisitor;

public class RemoveVisitorHandler : ICommandHandler<RemoveVisitorCommand, RemoveVisitorResponse>
{
    private readonly IVisitorService visitorService;

    public RemoveVisitorHandler(IVisitorService visitorService)
    {
        this.visitorService = visitorService;
    }

    public Task<RemoveVisitorResponse> ExecuteAsync(RemoveVisitorCommand command,
        CancellationToken ct = new CancellationToken())
    {
        visitorService.RemoveVisitor(command.ConnectionId);
        var response = new RemoveVisitorResponse { VisitorCount = visitorService.VisitorCount };
        return Task.FromResult(response);
    }
}
