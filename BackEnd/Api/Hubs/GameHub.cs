using MediatR;
using Microsoft.AspNetCore.SignalR;
using OhMyWord.Api.Extensions;
using OhMyWord.Domain.Contracts.Notifications;
using OhMyWord.Domain.Contracts.Requests;
using OhMyWord.Domain.Contracts.Results;
using OhMyWord.Domain.Models;
using OhMyWord.Domain.Services;

namespace OhMyWord.Api.Hubs;

public interface IGameHub
{
    Task SendRoundStarted(RoundStartedNotification notification, CancellationToken cancellationToken = default);
    Task SendRoundEnded(RoundSummary summary, CancellationToken cancellationToken = default);
    Task SendPlayerCount(int count);
    Task SendLetterHint(LetterHint letterHint);
}

public class GameHub : Hub<IGameHub>
{
    private readonly ILogger<GameHub> logger;
    private readonly IStateManager stateManager;
    private readonly IMediator mediator;

    public GameHub(ILogger<GameHub> logger, IStateManager stateManager, IMediator mediator)
    {
        this.logger = logger;
        this.stateManager = stateManager;
        this.mediator = mediator;
    }

    public override Task OnConnectedAsync()
        => mediator.Publish(new PlayerConnectedNotification(Context.ConnectionId, Context.GetIpAddress()));

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        if (exception is null)
            logger.LogDebug("Client disconnected. Connection ID: {ConnectionId}", Context.ConnectionId);
        else
            logger.LogError(exception, "Client disconnected. Connection ID: {ConnectionId}", Context.ConnectionId);

        return Task.WhenAll(
            mediator.Publish(new PlayerDisconnectedNotification(Context.ConnectionId)),
            Clients.Others.SendPlayerCount(stateManager.PlayerState.PlayerCount));
    }

    [HubMethodName("registerPlayer")]
    public async Task<RegisterPlayerResult> RegisterPlayerAsync(Guid playerId, string visitorId)
    {
        logger.LogInformation("Attempting to register player with visitor ID: {VisitorId}", visitorId);

        var request = new RegisterPlayerRequest(Context.ConnectionId, playerId, visitorId, Context.GetIpAddress(),
            Context.GetUserId());

        var result = await mediator.Send(request);

        await Clients.Others.SendPlayerCount(result.PlayerCount);

        return result;
    }

    [HubMethodName("submitGuess")]
    public async Task<ProcessGuessResult> ProcessGuessAsync(Guid roundId, string value)
    {
        var request = new ProcessGuessRequest(Context.ConnectionId, roundId, value);
        return await mediator.Send(request);
    }
}
