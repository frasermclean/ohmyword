using MediatR;
using WhatTheWord.Api.Responses.Game;

namespace WhatTheWord.Api.Mediator.Requests.Game;

public class GetHintRequest : IRequest<HintResponse>
{
    public string ClientId { get; init; } = default!;
}
