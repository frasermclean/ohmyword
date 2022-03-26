using MediatR;
using WhatTheWord.Api.Mediator.Requests.Words;
using WhatTheWord.Api.Responses.Words;
using WhatTheWord.Api.Services;
using WhatTheWord.Data.Repositories;

namespace WhatTheWord.Api.Mediator.Handlers.Words;

public class GetCurrentWordHandler : IRequestHandler<GetCurrentWordRequest, CurrentWordResponse>
{
    private readonly IGameService gameService;

    public GetCurrentWordHandler(IGameService gameService)
    {
        this.gameService = gameService;
    }

    public async Task<CurrentWordResponse> Handle(GetCurrentWordRequest request, CancellationToken cancellationToken)
    {
        var response = await gameService.GetCurrentWord();
        return response;
    }
}
