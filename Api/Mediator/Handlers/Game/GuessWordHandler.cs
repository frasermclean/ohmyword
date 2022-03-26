using MediatR;
using OhMyWord.Api.Mediator.Requests.Game;
using OhMyWord.Api.Responses.Game;
using OhMyWord.Api.Services;

namespace OhMyWord.Api.Mediator.Handlers.Game;

public class GuessWordHandler : IRequestHandler<GuessWordRequest, GuessWordResponse>
{
    private readonly IGameService gameService;

    public GuessWordHandler(IGameService gameService)
    {
        this.gameService = gameService;
    }

    public async Task<GuessWordResponse> Handle(GuessWordRequest wordRequest, CancellationToken cancellationToken)
    {
        var response = await gameService.TestGuessAsync(wordRequest);
        return response;
    }
}
