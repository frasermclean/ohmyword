using MediatR;
using OhMyWord.Core.Responses.Game;

namespace OhMyWord.Core.Requests.Game;

public class SubmitGuessRequest : IRequest<SubmitGuessResponse>
{
    public Guid RoundId { get; init; }
    public string Value { get; init; } = string.Empty;
    public string ConnectionId { get; init; } = string.Empty;
}
