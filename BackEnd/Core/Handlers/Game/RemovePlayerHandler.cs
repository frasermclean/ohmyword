using MediatR;
using OhMyWord.Core.Requests.Game;
using OhMyWord.Core.Responses.Game;
using OhMyWord.Core.Services;

namespace OhMyWord.Core.Handlers.Game;

public class RemoveVisitorHandler : RequestHandler<RemoveVisitorRequest, RemoveVisitorResponse>
{
    private readonly IVisitorService visitorService;

    public RemoveVisitorHandler(IVisitorService visitorService)
    {
        this.visitorService = visitorService;
    }

    protected override RemoveVisitorResponse Handle(RemoveVisitorRequest request)
    {
        visitorService.RemoveVisitor(request.ConnectionId);
        return new RemoveVisitorResponse
        {
            VisitorCount = visitorService.VisitorCount
        };
    }
}
