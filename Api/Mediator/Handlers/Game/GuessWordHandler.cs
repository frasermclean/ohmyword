using MediatR;
using WhatTheWord.Api.Mediator.Requests.Game;
using WhatTheWord.Api.Responses.Game;
using WhatTheWord.Api.Services;

namespace WhatTheWord.Api.Mediator.Handlers.Game;

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
