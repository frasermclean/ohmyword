using FastEndpoints;
using Microsoft.AspNetCore.SignalR;
using OhMyWord.Api.Commands.RegisterVisitor;
using OhMyWord.Core.Models;
using OhMyWord.Core.Responses.Game;

namespace OhMyWord.Api.Hubs;

public interface IGameHub
{
    Task SendLetterHint(LetterHint letterHint);
    Task SendRoundStarted(RoundStartResponse response);
    Task SendRoundEnded(RoundEndResponse response);
    Task SendVisitorCount(int count);
}

public class GameHub : Hub<IGameHub>
{
    private readonly ILogger<GameHub> logger;    

    public GameHub(ILogger<GameHub> logger)
    {
        this.logger = logger;        
    }

    public override async Task OnDisconnectedAsync(Exception? ex)
    {
        if (ex is null)
            logger.LogDebug("Client disconnected. Connection ID: {ConnectionId}", Context.ConnectionId);
        else
            logger.LogError(ex, "Client disconnected. Connection ID: {ConnectionId}", Context.ConnectionId);

        //var response = await mediator.Send(new RemoveVisitorRequest { ConnectionId = Context.ConnectionId });

        await Clients.Others.SendVisitorCount(0);
    }

    public async Task<RegisterVisitorResponse> RegisterVisitor(string visitorId)
    {
        logger.LogInformation("Attempting to register client with visitor ID: {VisitorId}", visitorId);
        var response = await new RegisterVisitorCommand
        {
            VisitorId = visitorId,
            ConnectionId = Context.ConnectionId
        }.ExecuteAsync();
        await Clients.Others.SendVisitorCount(response.VisitorCount);
        return response;
    }

    public Task<SubmitGuessResponse> SubmitGuess(Guid roundId, string value)
    {
        // return await mediator.Send(new SubmitGuessRequest
        // {
        //     RoundId = roundId,
        //     Value = value,
        //     ConnectionId = Context.ConnectionId
        // });
        return Task.FromResult(new SubmitGuessResponse());
    }
}
