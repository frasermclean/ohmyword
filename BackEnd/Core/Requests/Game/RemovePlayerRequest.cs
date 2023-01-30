using MediatR;
using OhMyWord.Core.Responses.Game;

namespace OhMyWord.Core.Requests.Game;

public class RemoveVisitorRequest : IRequest<RemoveVisitorResponse>
{
    public string ConnectionId { get; init; } = string.Empty;
}
