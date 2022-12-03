using MediatR;
using OhMyWord.Core.Responses.Game;

namespace OhMyWord.Core.Requests.Game;

public class RegisterPlayerRequest : IRequest<RegisterPlayerResponse>
{
    public string VisitorId { get; init; } = string.Empty;
    public string ConnectionId { get; init; } = string.Empty;
}