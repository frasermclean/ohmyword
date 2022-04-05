using Microsoft.AspNetCore.SignalR;
using OhMyWord.Api.Requests.Game;
using OhMyWord.Api.Responses.Game;
using OhMyWord.Core.Models;
using OhMyWord.Services.Game;

namespace OhMyWord.Api.Hubs;

public interface IGameHub
{
    Task SendHint(Hint hint);
    Task SendGameStatus(GameStatus status);
}

public class GameHub : Hub<IGameHub>
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
        await Clients.Caller.SendHint(gameService.GetHint());
        return new RegisterPlayerResponse
        {
            PlayerId = player.Id,
            Status = gameService.GetGameStatus()
        };
    }

    public Hint GetHint(string playerId)
    {
        return gameService.GetHint();
    }

    public GameStatus GetStatus(string playerId)
    {
        return gameService.GetGameStatus();
    }

    public GuessWordResponse GuessWord(GuessWordRequest request)
    {
        var isCorrect = gameService.IsCorrect(request.Value);
        return new GuessWordResponse
        {
            Value = request.Value.ToLowerInvariant(),
            Correct = isCorrect,
        };
    }
}
