using MediatR;
using OhMyWord.Api.Responses.Game;

namespace OhMyWord.Api.Mediator.Requests.Game;

public class GetHintRequest : IRequest<HintResponse>
{
    public string ClientId { get; init; } = default!;
}
