using MediatR;
using WhatTheWord.Domain.Requests.Game;
using WhatTheWord.Domain.Responses.Game;
using WhatTheWord.Domain.Services;

namespace WhatTheWord.Domain.Handlers.Game;

public class TryGuessHandler : IRequestHandler<TryGuessRequest, TryGuessResponse>
{
    private readonly IGameService gameService;

    public TryGuessHandler(IGameService gameService)
    {
        this.gameService = gameService;
    }

    public async Task<TryGuessResponse> Handle(TryGuessRequest request, CancellationToken cancellationToken)
    {
        var response = await gameService.TryGuessAsync(request);
        return response;
    }
}
