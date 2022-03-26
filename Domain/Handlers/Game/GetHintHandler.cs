using MediatR;
using WhatTheWord.Domain.Requests.Game;
using WhatTheWord.Domain.Responses.Game;
using WhatTheWord.Domain.Services;

namespace WhatTheWord.Domain.Handlers.Game;

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
