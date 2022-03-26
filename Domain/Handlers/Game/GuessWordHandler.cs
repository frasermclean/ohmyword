using MediatR;
using WhatTheWord.Domain.Requests.Game;
using WhatTheWord.Domain.Responses.Game;
using WhatTheWord.Domain.Services;

namespace WhatTheWord.Domain.Handlers.Game;

public class GuessWordHandler : IRequestHandler<GuessWordRequest, GuessWordResponse>
{
    private readonly IGameService gameService;

    public GuessWordHandler(IGameService gameService)
    {
        this.gameService = gameService;
    }

    public async Task<GuessWordResponse> Handle(GuessWordRequest wordRequest, CancellationToken cancellationToken)
    {
        var response = await gameService.GuessWordAsync(wordRequest);
        return response;
    }
}
