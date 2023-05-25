using MediatR;
using Microsoft.AspNetCore.SignalR;
using OhMyWord.Api.Extensions;
using OhMyWord.Api.Models;
using OhMyWord.Api.Services;
using OhMyWord.Domain.Contracts.Notifications;
using OhMyWord.Domain.Models;
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
    private readonly IStateManager stateManager;
    private readonly IPlayerInputService playerInputService;
    private readonly IPublisher publisher;

    public GameHub(ILogger<GameHub> logger, IStateManager stateManager, IPlayerInputService playerInputService,
        IPublisher publisher)
    {
        this.logger = logger;
        this.stateManager = stateManager;
        this.playerInputService = playerInputService;
        this.publisher = publisher;
    }

    public override Task OnConnectedAsync()
        => publisher.Publish(new PlayerConnectedNotification(Context.ConnectionId, Context.GetIpAddress()));

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        if (exception is null)
            logger.LogDebug("Client disconnected. Connection ID: {ConnectionId}", Context.ConnectionId);
        else
            logger.LogError(exception, "Client disconnected. Connection ID: {ConnectionId}", Context.ConnectionId);

        return Task.WhenAll(
            publisher.Publish(new PlayerDisconnectedNotification(Context.ConnectionId)),
            Clients.Others.SendPlayerCount(stateManager.PlayerState.PlayerCount));
    }

    [HubMethodName("registerPlayer")]
    public async Task<PlayerRegisteredResult> RegisterPlayerAsync(Guid playerId, string visitorId)
    {
        logger.LogInformation("Attempting to register player with visitor ID: {VisitorId}", visitorId);

        var result = await playerInputService.RegisterPlayerAsync(Context.ConnectionId, visitorId,
            Context.GetIpAddress(), Context.GetUserId());

        await Clients.Others.SendPlayerCount(result.PlayerCount);

        return result;
    }

    [HubMethodName("submitGuess")]
    public async Task<GuessProcessedResult> ProcessGuessAsync(Guid roundId, string value)
    {
        var result = await playerInputService.ProcessGuessAsync(Context.ConnectionId, roundId, value);
        return result;
    }
}
