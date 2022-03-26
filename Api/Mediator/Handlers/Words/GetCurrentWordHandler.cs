using MediatR;
using OhMyWord.Api.Mediator.Requests.Words;
using OhMyWord.Api.Responses.Words;
using OhMyWord.Api.Services;
using OhMyWord.Data.Repositories;

namespace OhMyWord.Api.Mediator.Handlers.Words;

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
