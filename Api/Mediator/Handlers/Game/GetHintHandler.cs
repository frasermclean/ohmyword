using MediatR;
using WhatTheWord.Api.Mediator.Requests.Game;
using WhatTheWord.Api.Responses.Game;
using WhatTheWord.Api.Services;

namespace WhatTheWord.Api.Mediator.Handlers.Game;

public class GetHintHandler : IRequestHandler<GetHintRequest, HintResponse>
{
    private readonly IGameService gameService;

    public GetHintHandler(IGameService gameService)
    {
        this.gameService = gameService;
    }

    public async Task<HintResponse> Handle(GetHintRequest request, CancellationToken cancellationToken)
    {
        var (word, expiry) = await gameService.GetCurrentWord();
        var response = new HintResponse(word, expiry);
        return response;
    }
}
