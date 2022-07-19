using MediatR;
using OhMyWord.Core.Requests.Game;
using OhMyWord.Core.Responses.Game;
using OhMyWord.Core.Services;

namespace OhMyWord.Core.Handlers.Game;

public class RemovePlayerHandler : RequestHandler<RemovePlayerRequest, RemovePlayerResponse>
{
    private readonly IPlayerService playerService;

    public RemovePlayerHandler(IPlayerService playerService)
    {
        this.playerService = playerService;
    }

    protected override RemovePlayerResponse Handle(RemovePlayerRequest request)
    {
        playerService.RemovePlayer(request.ConnectionId);
        return new RemovePlayerResponse
        {
            PlayerCount = playerService.PlayerCount
        };
    }
}