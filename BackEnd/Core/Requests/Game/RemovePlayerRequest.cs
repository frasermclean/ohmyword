using MediatR;
using OhMyWord.Core.Responses.Game;

namespace OhMyWord.Core.Requests.Game;

public class RemovePlayerRequest : IRequest<RemovePlayerResponse>
{
    public string ConnectionId { get; init; } = string.Empty;
}