using Microsoft.AspNetCore.SignalR;
using OhMyWord.Api.Commands.RegisterPlayer;
using OhMyWord.Api.Commands.SubmitGuess;
using OhMyWord.Api.Events.PlayerDisconnected;
using OhMyWord.Domain.Extensions;
using OhMyWord.Domain.Models;
using OhMyWord.Domain.Services;

namespace OhMyWord.Api.Hubs;

public interface IGameHub
{
    Task SendGameState(GameState gameState);
    Task SendPlayerCount(int count);
    Task SendLetterHint(LetterHint letterHint);
}

public class GameHub : Hub<IGameHub>
{
    private readonly ILogger<GameHub> logger;
    private readonly IPlayerService playerService;

    public GameHub(ILogger<GameHub> logger, IPlayerService playerService)
    {
        this.logger = logger;
        this.playerService = playerService;
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (exception is null)
            logger.LogDebug("Client disconnected. Connection ID: {ConnectionId}", Context.ConnectionId);
        else
            logger.LogError(exception, "Client disconnected. Connection ID: {ConnectionId}", Context.ConnectionId);

        await new PlayerDisconnectedEvent(Context.ConnectionId).PublishAsync();
        await Clients.Others.SendPlayerCount(playerService.PlayerCount);
    }

    public async Task<RegisterPlayerResponse> RegisterPlayer(string visitorId)
    {
        var userId = Context.User?.GetUserId();

        logger.LogInformation("Attempting to register player with visitor ID: {VisitorId}, user ID: {UserId}",
            visitorId, userId);

        var response = await new RegisterPlayerCommand
            {
                VisitorId = visitorId, ConnectionId = Context.ConnectionId, UserId = userId
            }
            .ExecuteAsync();

        await Clients.Others.SendPlayerCount(response.PlayerCount);
        return response;
    }

    public Task<SubmitGuessResponse> SubmitGuess(Guid roundId, string value)
        => new SubmitGuessCommand { RoundId = roundId, Value = value, ConnectionId = Context.ConnectionId, }
            .ExecuteAsync();
}
