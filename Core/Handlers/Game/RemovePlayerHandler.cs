using FluentValidation;
using MediatR;
using OhMyWord.Core.Requests.Game;
using OhMyWord.Core.Responses.Game;
using OhMyWord.Core.Services;

namespace OhMyWord.Core.Handlers.Game;

public class RemovePlayerHandler : RequestHandler<RemovePlayerRequest, RemovePlayerResponse>
{
    private readonly IPlayerService playerService;
    private readonly IValidator<RemovePlayerRequest> validator;

    public RemovePlayerHandler(IPlayerService playerService, IValidator<RemovePlayerRequest> validator)
    {
        this.playerService = playerService;
        this.validator = validator;
    }    

    protected override RemovePlayerResponse Handle(RemovePlayerRequest request)
    {
        validator.ValidateAndThrow(request);
        playerService.RemovePlayer(request.ConnectionId);
        return new RemovePlayerResponse
        {
            PlayerCount = playerService.PlayerCount
        };
    }
}