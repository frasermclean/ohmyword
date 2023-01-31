﻿using FastEndpoints;
using Microsoft.AspNetCore.SignalR;
using OhMyWord.Api.Commands.RegisterVisitor;
using OhMyWord.Api.Commands.SubmitGuess;
using OhMyWord.Api.Events.VisitorDisconnected;
using OhMyWord.Api.Services;
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
    private readonly IVisitorService visitorService;

    public GameHub(ILogger<GameHub> logger, IVisitorService visitorService)
    {
        this.logger = logger;
        this.visitorService = visitorService;
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (exception is null)
            logger.LogDebug("Client disconnected. Connection ID: {ConnectionId}", Context.ConnectionId);
        else
            logger.LogError(exception, "Client disconnected. Connection ID: {ConnectionId}", Context.ConnectionId);

        await new VisitorDisconnectedEvent(Context.ConnectionId).PublishAsync();
        await Clients.Others.SendVisitorCount(visitorService.VisitorCount);
    }

    public async Task<RegisterVisitorResponse> RegisterVisitor(string visitorId)
    {
        logger.LogInformation("Attempting to register client with visitor ID: {VisitorId}", visitorId);
        var response = await new RegisterVisitorCommand { VisitorId = visitorId, ConnectionId = Context.ConnectionId }
            .ExecuteAsync();
        await Clients.Others.SendVisitorCount(response.VisitorCount);
        return response;
    }

    public Task<SubmitGuessResponse> SubmitGuess(Guid roundId, string value)
        => new SubmitGuessCommand { RoundId = roundId, Value = value, ConnectionId = Context.ConnectionId, }
            .ExecuteAsync();
}
