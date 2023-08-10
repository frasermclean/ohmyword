using Microsoft.AspNetCore.SignalR;
using OhMyWord.Api.Extensions;
using OhMyWord.Api.Models;
using OhMyWord.Domain.Contracts.Commands;
using OhMyWord.Domain.Contracts.Events;
using OhMyWord.Domain.Models;
using OhMyWord.Domain.Services.State;

namespace OhMyWord.Api.Hubs;

public interface IGameHub
{
    Task SendRoundStarted(RoundStartedResponse response, CancellationToken cancellationToken = default);
    Task SendRoundEnded(RoundEndedResponse summary, CancellationToken cancellationToken = default);
    Task SendPlayerCount(int count, CancellationToken cancellationToken = default);
    Task SendLetterHint(LetterHintResponse letterHint, CancellationToken cancellationToken = default);
}

public class GameHub : Hub<IGameHub>
{
    private readonly ILogger<GameHub> logger;
    private readonly IRootState rootState;

    public GameHub(ILogger<GameHub> logger, IRootState rootState)
    {
        this.logger = logger;
        this.rootState = rootState;
    }

    public override async Task OnConnectedAsync()
    {
        logger.LogInformation("Client connected. Connection ID: {ConnectionId}", Context.ConnectionId);
        await new PlayerConnectedEvent(Context.ConnectionId, Context.GetIpAddress()).PublishAsync(Mode.WaitForNone);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (exception is null)
            logger.LogInformation("Client disconnected. Connection ID: {ConnectionId}", Context.ConnectionId);
        else
            logger.LogWarning(exception, "Client disconnected. Connection ID: {ConnectionId}", Context.ConnectionId);

        await new PlayerDisconnectedEvent(Context.ConnectionId).PublishAsync();
        await Clients.Others.SendPlayerCount(rootState.PlayerState.PlayerCount);
    }

    [HubMethodName("registerPlayer")]
    public async Task<RegisterPlayerResponse> RegisterPlayerAsync(Guid playerId, string visitorId)
    {
        logger.LogInformation("Attempting to register player with visitor ID: {VisitorId}", visitorId);

        var result = await new RegisterPlayerCommand(Context.ConnectionId, playerId, visitorId, Context.GetIpAddress(),
            Context.User?.GetUserId()).ExecuteAsync();

        await Clients.Others.SendPlayerCount(result.PlayerCount);

        return RegisterPlayerResponse.FromRegisterPlayerResult(result);
    }

    [HubMethodName("submitGuess")]
    public async Task<ProcessGuessResult> ProcessGuessAsync(Guid roundId, string value)
    {
        var result = await new ProcessGuessCommand(Context.ConnectionId, roundId, value).ExecuteAsync();
        return result;
    }
}
