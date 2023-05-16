using Microsoft.AspNetCore.SignalR;
using OhMyWord.Api.Commands.RegisterPlayer;
using OhMyWord.Api.Commands.SubmitGuess;
using OhMyWord.Api.Events.PlayerConnected;
using OhMyWord.Api.Events.PlayerDisconnected;
using OhMyWord.Api.Extensions;
using OhMyWord.Domain.Models;
using OhMyWord.Domain.Services;

namespace OhMyWord.Api.Hubs;

public interface IGameHub
{
    Task SendRoundStarted(RoundStartData data, CancellationToken cancellationToken = default);
    Task SendRoundEnded(RoundEndData data, CancellationToken cancellationToken = default);
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

    public async Task<RegisterPlayerResponse> RegisterPlayer(string visitorId)
    {
        logger.LogInformation("Attempting to register player with visitor ID: {VisitorId}", visitorId);

        var response = await new RegisterPlayerCommand
            {
                VisitorId = visitorId,
                ConnectionId = Context.ConnectionId,
                UserId = Context.GetUserId(),
                IpAddress = Context.GetIpAddress()
            }
            .ExecuteAsync();

        await Clients.Others.SendPlayerCount(response.PlayerCount);

        return response;
    }

    public Task<SubmitGuessResponse> SubmitGuess(Guid roundId, string value)
        => new SubmitGuessCommand { RoundId = roundId, Value = value, ConnectionId = Context.ConnectionId, }
            .ExecuteAsync();
}
