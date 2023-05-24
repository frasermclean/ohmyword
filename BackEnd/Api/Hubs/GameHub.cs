using Microsoft.AspNetCore.SignalR;
using OhMyWord.Api.Events.PlayerConnected;
using OhMyWord.Api.Events.PlayerDisconnected;
using OhMyWord.Api.Extensions;
using OhMyWord.Api.Models;
using OhMyWord.Api.Services;
using OhMyWord.Domain.Models;
using OhMyWord.Domain.Models.Notifications;
using OhMyWord.Domain.Services;

namespace OhMyWord.Api.Hubs;

public interface IGameHub
{
    Task SendRoundStarted(RoundStartedNotification notification, CancellationToken cancellationToken = default);
    Task SendRoundEnded(RoundEndedNotification notification, CancellationToken cancellationToken = default);
    Task SendPlayerCount(int count);
    Task SendLetterHint(LetterHint letterHint);
}

public class GameHub : Hub<IGameHub>
{
    private readonly ILogger<GameHub> logger;
    private readonly IGameService gameService;
    private readonly IPlayerService playerService;

    public GameHub(ILogger<GameHub> logger, IGameService gameService, IPlayerService playerService)
    {
        this.logger = logger;
        this.gameService = gameService;
        this.playerService = playerService;
    }

    public override async Task OnConnectedAsync()
    {
        await new PlayerConnectedEvent(Context.ConnectionId, Context.GetIpAddress())
            .PublishAsync(Mode.WaitForNone);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (exception is null)
            logger.LogDebug("Client disconnected. Connection ID: {ConnectionId}", Context.ConnectionId);
        else
            logger.LogError(exception, "Client disconnected. Connection ID: {ConnectionId}", Context.ConnectionId);

        await new PlayerDisconnectedEvent(Context.ConnectionId).PublishAsync(Mode.WaitForNone);
        await Clients.Others.SendPlayerCount(playerService.PlayerCount);
    }

    [HubMethodName("registerPlayer")]
    public async Task<RegisterPlayerResult> RegisterPlayerAsync(string visitorId)
    {
        logger.LogInformation("Attempting to register player with visitor ID: {VisitorId}", visitorId);

        var result = await gameService.RegisterPlayerAsync(Context.ConnectionId, visitorId, Context.GetIpAddress(),
            Context.GetUserId());

        await Clients.Others.SendPlayerCount(result.PlayerCount);

        return result;
    }

    [HubMethodName("submitGuess")]
    public async Task<ProcessGuessResult> ProcessGuessAsync(Guid roundId, string value)
    {
        var result = await gameService.ProcessGuessAsync(Context.ConnectionId, roundId, value);
        return result;
    }
}
