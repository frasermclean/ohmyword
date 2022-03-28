using Microsoft.AspNetCore.SignalR;
using Microsoft.Azure.Cosmos.Linq;
using OhMyWord.Api.Requests.Game;
using OhMyWord.Api.Responses.Game;
using OhMyWord.Api.Services;

namespace OhMyWord.Api.Hubs;

public interface IGameHub
{
    Task SendHint(Hint hint);
}

public class GameHub : Hub<IGameHub>, IGameHub
{
    private readonly ILogger<GameHub> logger;
    private readonly IGameService gameService;

    public GameHub(ILogger<GameHub> logger, IGameService gameService)
    {
        this.logger = logger;
        this.gameService = gameService;
    }

    public override Task OnConnectedAsync()
    {
        logger.LogDebug("New client connected from connection ID: {connectionId}", Context.ConnectionId);
        return base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        logger.LogDebug("Client disconnected. Connection ID: {connectionId}", Context.ConnectionId);
        await gameService.UnregisterPlayerAsync(Context.ConnectionId);
    }

    public async Task<RegisterPlayerResponse> RegisterPlayer(string visitorId)
    {
        logger.LogInformation("Attempting to register client with visitor ID: {visitorId}", visitorId);
        var player = await gameService.RegisterPlayerAsync(visitorId, Context.ConnectionId);
        return new RegisterPlayerResponse(player);
    }

    public Hint GetHint(GetHintRequest request)
    {
        return gameService.CurrentHint;
    }

    public async Task<GuessWordResponse> GuessWord(GuessWordRequest request)
    {
        var response = await gameService.TestPlayerGuess(request);
        return response;
    }

    public Task SendHint(Hint hint) => Clients.All.SendHint(hint);
}
