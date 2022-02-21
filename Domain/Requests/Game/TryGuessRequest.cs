using MediatR;
using WhatTheWord.Domain.Responses.Game;

namespace WhatTheWord.Domain.Requests.Game;

public class TryGuessRequest : IRequest<TryGuessResponse>
{
    public string ClientId { get; set; } = default!;
    public string Guess { get; init; } = default!;
}
