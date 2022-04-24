﻿using Microsoft.AspNetCore.SignalR;
using OhMyWord.Api.Requests.Game;
using OhMyWord.Api.Responses.Game;
using OhMyWord.Core.Models;
using OhMyWord.Services.Game;
using OhMyWord.Services.Models;

namespace OhMyWord.Api.Hubs;

public interface IGameHub
{
    Task SendLetterHint(LetterHint letterHint);
    Task SendRoundStarted(RoundStart roundStart);
    Task SendRoundEnded(RoundEnd roundEnd);
}

public class GameHub : Hub<IGameHub>
{
    private readonly ILogger<GameHub> logger;
    private readonly IGameService gameService;

    public GameHub(ILogger<GameHub> logger, IGameService gameService)
    {
        this.logger = logger;
        this.gameService = gameService;
    }

    public override Task OnConnectedAsync()
    {
        logger.LogDebug("New client connected from connection ID: {connectionId}", Context.ConnectionId);
        return base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        logger.LogDebug("Client disconnected. Connection ID: {connectionId}", Context.ConnectionId);
        await gameService.RemovePlayerAsync(Context.ConnectionId);
    }

    public async Task<RegisterPlayerResponse> RegisterPlayer(string visitorId)
    {
        logger.LogInformation("Attempting to register client with visitor ID: {visitorId}", visitorId);
        var player = await gameService.AddPlayerAsync(visitorId, Context.ConnectionId);
        return new RegisterPlayerResponse
        {
            PlayerId = player.Id,
            RoundNumber = gameService.RoundNumber,
            RoundActive = gameService.RoundActive,
            WordHint = gameService.Round?.WordHint,
            PlayerCount = gameService.PlayerCount,
            Expiry = gameService.Expiry,
            Score = player.Score
        };
    }
    
    public async Task<SubmitGuessResponse> SubmitGuess(SubmitGuessRequest request)
    {
        var points = await gameService.ProcessGuessAsync(request.PlayerId, request.Value);
        return new SubmitGuessResponse
        {
            Value = request.Value.ToLowerInvariant(),
            Correct = points > 0,
            Points = points
        };
    }
}
