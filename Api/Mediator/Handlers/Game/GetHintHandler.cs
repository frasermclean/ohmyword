using MediatR;
using OhMyWord.Api.Mediator.Requests.Game;
using OhMyWord.Api.Responses.Game;
using OhMyWord.Api.Services;

namespace OhMyWord.Api.Mediator.Handlers.Game;

public class GetHintHandler : IRequestHandler<GetHintRequest, HintResponse>
{
    private readonly IGameService gameService;

    public GetHintHandler(IGameService gameService)
    {
        this.gameService = gameService;
    }

    public async Task<HintResponse> Handle(GetHintRequest request, CancellationToken cancellationToken)
    {
        var response = await gameService.GetCurrentWord();
        return new HintResponse(response.Word, response.Expiry);
        
    }
}
