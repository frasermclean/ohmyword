using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using OhMyWord.Api.Commands;
using OhMyWord.Api.Commands.RegisterPlayer;
using OhMyWord.Core.Models;
using OhMyWord.Core.Requests.Game;
using OhMyWord.Core.Responses.Game;

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

    public GameHub(ILogger<GameHub> logger, IMediator mediator)
    {
        this.logger = logger;
        this.mediator = mediator;
    }

    public override async Task OnDisconnectedAsync(Exception? ex)
    {
        if (ex is null)
            logger.LogDebug("Client disconnected. Connection ID: {ConnectionId}", Context.ConnectionId);
        else
            logger.LogError(ex, "Client disconnected. Connection ID: {ConnectionId}", Context.ConnectionId);

        var response = await mediator.Send(new RemovePlayerRequest { ConnectionId = Context.ConnectionId });

        await Clients.Others.SendPlayerCount(response.PlayerCount);
    }

    public async Task<RegisterPlayerResponse> RegisterPlayer(string visitorId)
    {
        logger.LogInformation("Attempting to register client with visitor ID: {VisitorId}", visitorId);
        var response = await new RegisterPlayerCommand
        {
            VisitorId = visitorId,
            ConnectionId = Context.ConnectionId
        }.ExecuteAsync();
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
