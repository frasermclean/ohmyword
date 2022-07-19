using MediatR;
using Microsoft.AspNetCore.SignalR;
using OhMyWord.Core.Requests.Game;
using OhMyWord.Core.Responses.Game;
using OhMyWord.Core.Services;
using OhMyWord.Data.Models;

namespace OhMyWord.Api.Hubs;

public interface IGameHub
{
    Task SendLetterHint(LetterHint letterHint);
    Task SendRoundStarted(RoundStartResponse response);
    Task SendRoundEnded(RoundEndResponse response);
    Task SendPlayerCount(int count);
}

public class GameHub : Hub<IGameHub>
{
    private readonly ILogger<GameHub> logger;
    private readonly IMediator mediator;    
    private readonly IPlayerService playerService;

    public GameHub(ILogger<GameHub> logger, IMediator mediator, IPlayerService playerService)
    {
        this.logger = logger;
        this.mediator = mediator;        
        this.playerService = playerService;
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        logger.LogDebug("Client disconnected. Connection ID: {connectionId}", Context.ConnectionId);
        playerService.RemovePlayer(Context.ConnectionId);
        await Clients.Others.SendPlayerCount(playerService.PlayerCount);
    }

    public async Task<RegisterPlayerResponse> RegisterPlayer(string visitorId)
    {
        logger.LogInformation("Attempting to register client with visitor ID: {visitorId}", visitorId);
        var response = await mediator.Send(new RegisterPlayerRequest
        {
            VisitorId = visitorId,
            ConnectionId = Context.ConnectionId
        });
        await Clients.Others.SendPlayerCount(response.PlayerCount);
        return response;
    }

    public async Task<SubmitGuessResponse> SubmitGuess(Guid roundId, string value)
    {
        return await mediator.Send(new SubmitGuessRequest
        {
            RoundId = roundId,
            Value = value,
            ConnectionId = Context.ConnectionId
        });        
    }
}