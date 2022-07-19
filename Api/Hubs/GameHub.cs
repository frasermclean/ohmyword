using Microsoft.AspNetCore.SignalR;
using OhMyWord.Api.Requests.Game;
using OhMyWord.Api.Responses.Game;
using OhMyWord.Core.Game;
using OhMyWord.Data.Models;

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
    private readonly IGameService gameService;
    private readonly IPlayerService playerService;

    public GameHub(ILogger<GameHub> logger, IGameService gameService, IPlayerService playerService)
    {
        this.logger = logger;
        this.gameService = gameService;
        this.playerService = playerService;
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        logger.LogDebug("Client disconnected. Connection ID: {connectionId}", Context.ConnectionId);
        playerService.RemovePlayer(Context.ConnectionId);
        await Clients.Others.SendPlayerCount(playerService.PlayerCount);
    }

    public async Task<RegisterPlayerResponse> RegisterPlayer(string visitorId)
    {
        logger.LogInformation("Attempting to register client with visitor ID: {visitorId}", visitorId);
        var player = await playerService.AddPlayerAsync(visitorId, Context.ConnectionId);
        await Clients.Others.SendPlayerCount(playerService.PlayerCount);
        return new RegisterPlayerResponse
        {
            RoundActive = gameService.RoundActive,
            PlayerCount = playerService.PlayerCount,
            Score = player.Score,
            RoundStart = gameService.RoundActive ? new RoundStartResponse
                {
                    RoundNumber = gameService.Round.Number,
                    RoundId = gameService.Round.Id,
                    RoundStarted = gameService.Round.StartTime,
                    RoundEnds = gameService.Round.EndTime,
                    WordHint = gameService.Round.WordHint,
                } : null,
            RoundEnd = !gameService.RoundActive ? new RoundEndResponse
                {
                    RoundNumber = gameService.Round.Number,
                    RoundId = gameService.Round.Id,
                    Word = gameService.Round.Word.Value,
                    EndReason = gameService.Round.EndReason,
                    NextRoundStart = gameService.Expiry,
                } : null,
        };
}

public async Task<SubmitGuessResponse> SubmitGuess(SubmitGuessRequest request)
{
    var (roundId, value) = request;
    var points = await gameService.ProcessGuessAsync(Context.ConnectionId, roundId, value);
    return new SubmitGuessResponse
    {
        Value = value.ToLowerInvariant(),
        Correct = points > 0,
        Points = points
    };
}
}
