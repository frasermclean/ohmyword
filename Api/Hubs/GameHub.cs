using Microsoft.AspNetCore.SignalR;
using OhMyWord.Api.Requests.Game;
using OhMyWord.Api.Responses.Game;
using OhMyWord.Core.Models;
using OhMyWord.Services.Game;

namespace OhMyWord.Api.Hubs;

public interface IGameHub
{
    Task SendLetterHint(LetterHint letterHint);
    Task SendRoundStarted(RoundStartResponse response);
    Task SendRoundEnded(RoundEndResponse response);
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

    public override Task OnConnectedAsync()
    {
        logger.LogDebug("New client connected from connection ID: {connectionId}", Context.ConnectionId);
        return base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        logger.LogDebug("Client disconnected. Connection ID: {connectionId}", Context.ConnectionId);
        await playerService.RemovePlayerAsync(Context.ConnectionId);
    }

    public async Task<RegisterPlayerResponse> RegisterPlayer(string visitorId)
    {
        logger.LogInformation("Attempting to register client with visitor ID: {visitorId}", visitorId);
        var player = await playerService.AddPlayerAsync(visitorId, Context.ConnectionId);
        return new RegisterPlayerResponse
        {
            PlayerId = player.Id,
            RoundActive = gameService.RoundActive,
            RoundNumber = gameService.RoundNumber,
            RoundId = gameService.Round.Id.ToString(),
            WordHint = gameService.Round.WordHint,
            PlayerCount = playerService.PlayerCount,
            Expiry = gameService.Expiry,
            Score = player.Score
        };
    }

    public async Task<SubmitGuessResponse> SubmitGuess(SubmitGuessRequest request)
    {
        var (playerId, roundId, value) = request;
        var points = await gameService.ProcessGuessAsync(playerId, roundId, value);
        return new SubmitGuessResponse
        {
            Value = value.ToLowerInvariant(),
            Correct = points > 0,
            Points = points
        };
    }
}
