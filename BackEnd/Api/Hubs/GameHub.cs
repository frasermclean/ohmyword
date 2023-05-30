using Microsoft.AspNetCore.SignalR;
using OhMyWord.Api.Extensions;
using OhMyWord.Domain.Contracts.Commands;
using OhMyWord.Domain.Contracts.Events;
using OhMyWord.Domain.Contracts.Results;
using OhMyWord.Domain.Models;
using OhMyWord.Domain.Services;

namespace OhMyWord.Api.Hubs;

public interface IGameHub
{
    Task SendRoundStarted(RoundStartedEvent eventModel, CancellationToken cancellationToken = default);
    Task SendRoundEnded(RoundSummary summary, CancellationToken cancellationToken = default);
    Task SendPlayerCount(int count, CancellationToken cancellationToken = default);
    Task SendLetterHint(LetterHint letterHint, CancellationToken cancellationToken = default);
}

public class GameHub : Hub<IGameHub>
{
    private readonly ILogger<GameHub> logger;
    private readonly IStateManager stateManager;

    public GameHub(ILogger<GameHub> logger, IStateManager stateManager)
    {
        this.logger = logger;
        this.stateManager = stateManager;
    }

    public override async Task OnConnectedAsync()
    {
        logger.LogInformation("Client connected. Connection ID: {ConnectionId}", Context.ConnectionId);
        await new PlayerConnectedEvent(Context.ConnectionId, Context.GetIpAddress()).PublishAsync(Mode.WaitForNone);
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        if (exception is null)
            logger.LogDebug("Client disconnected. Connection ID: {ConnectionId}", Context.ConnectionId);
        else
            logger.LogError(exception, "Client disconnected. Connection ID: {ConnectionId}", Context.ConnectionId);

        return Task.WhenAll(
            new PlayerDisconnectedEvent(Context.ConnectionId).PublishAsync(),
            Clients.Others.SendPlayerCount(stateManager.PlayerState.PlayerCount));
    }

    [HubMethodName("registerPlayer")]
    public async Task<RegisterPlayerResult> RegisterPlayerAsync(Guid playerId, string visitorId)
    {
        logger.LogInformation("Attempting to register player with visitor ID: {VisitorId}", visitorId);

        var result = await new RegisterPlayerCommand(Context.ConnectionId, playerId, visitorId, Context.GetIpAddress(),
            Context.GetUserId()).ExecuteAsync();

        await Clients.Others.SendPlayerCount(result.PlayerCount);

        return result;
    }

    [HubMethodName("submitGuess")]
    public async Task<ProcessGuessResult> ProcessGuessAsync(Guid roundId, string value)
    {
        var result = await new ProcessGuessCommand(Context.ConnectionId, roundId, value).ExecuteAsync();
        return result;
    }
}
