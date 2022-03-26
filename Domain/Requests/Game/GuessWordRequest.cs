using MediatR;
using WhatTheWord.Domain.Responses.Game;

namespace WhatTheWord.Domain.Requests.Game;

public class GuessWordRequest : IRequest<GuessWordResponse>
{
    public string ClientId { get; set; } = default!;
    public string Value { get; init; } = default!;
}
