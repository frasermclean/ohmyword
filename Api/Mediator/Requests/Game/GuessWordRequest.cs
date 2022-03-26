using MediatR;
using OhMyWord.Api.Responses.Game;

namespace OhMyWord.Api.Mediator.Requests.Game;

public class GuessWordRequest : IRequest<GuessWordResponse>
{
    public string ClientId { get; set; } = default!;
    public string Value { get; init; } = default!;
}
