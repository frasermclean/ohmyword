using MediatR;
using WhatTheWord.Api.Responses.Game;

namespace WhatTheWord.Api.Mediator.Requests.Game;

public class GuessWordRequest : IRequest<GuessWordResponse>
{
    public string ClientId { get; set; } = default!;
    public string Value { get; init; } = default!;
}
