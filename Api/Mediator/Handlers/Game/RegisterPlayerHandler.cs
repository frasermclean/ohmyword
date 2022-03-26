using MediatR;
using OhMyWord.Api.Mediator.Requests.Game;
using OhMyWord.Api.Responses.Game;
using OhMyWord.Api.Services;

namespace OhMyWord.Api.Mediator.Handlers.Game;

public class RegisterPlayerHandler : IRequestHandler<RegisterPlayerRequest, RegisterPlayerResponse>
{
    private readonly IGameService gameService;

    public RegisterPlayerHandler(IGameService gameService)
    {
        this.gameService = gameService;
    }

    public async Task<RegisterPlayerResponse> Handle(RegisterPlayerRequest request, CancellationToken cancellationToken)
    {
        return await gameService.RegisterPlayerAsync(request);
    }
}
