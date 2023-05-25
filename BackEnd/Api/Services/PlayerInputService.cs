using OhMyWord.Api.Models;
using OhMyWord.Domain.Models;
using OhMyWord.Domain.Services;
using OhMyWord.Infrastructure.Models.Entities;
using System.Net;

namespace OhMyWord.Api.Services;

public interface IPlayerInputService
{
    Task<PlayerRegisteredResult> RegisterPlayerAsync(string connectionId, string visitorId, IPAddress ipAddress,
        Guid? userId);

    Task<GuessProcessedResult> ProcessGuessAsync(string connectionId, Guid roundId, string value);
}

public class PlayerInputService : IPlayerInputService
{
    private readonly ILogger<PlayerInputService> logger;
    private readonly IPlayerService playerService;
    private readonly IStateManager stateManager;

    public PlayerInputService(ILogger<PlayerInputService> logger, IPlayerService playerService,
        IStateManager stateManager)
    {
        this.logger = logger;
        this.playerService = playerService;
        this.stateManager = stateManager;
    }

    public async Task<PlayerRegisteredResult> RegisterPlayerAsync(string connectionId, string visitorId,
        IPAddress ipAddress, Guid? userId)
    {
        var player = await playerService.GetOrCreatePlayerAsync(visitorId, connectionId, ipAddress, userId);
        var isSuccessful = stateManager.PlayerState.AddPlayer(player);

        return new PlayerRegisteredResult
        {
            IsSuccessful = isSuccessful,
            PlayerCount = stateManager.PlayerState.PlayerCount,
            Score = player.Score,
            StateSnapshot = GetGameState()
        };
    }

    public async Task<GuessProcessedResult> ProcessGuessAsync(string connectionId, Guid roundId, string value)
    {
        // validate round state
        var round = stateManager.Round;
        if (round is null) return GuessProcessedResult.Default;
        if (stateManager.SessionState != SessionState.RoundActive || roundId != round.Id)
            return GuessProcessedResult.Default;

        // compare value to current word value
        var isCorrect = string.Equals(value, round.Word.Id, StringComparison.InvariantCultureIgnoreCase);
        if (!isCorrect) return GuessProcessedResult.Default;

        var player = stateManager.PlayerState.GetPlayerByConnectionId(connectionId);
        if (player is null)
        {
            logger.LogError("Player not found. ConnectionId: {ConnectionId}", connectionId);
            return GuessProcessedResult.Default;
        }

        round.IncrementGuessCount(player.Id);

        const int pointsToAward = 100; // TODO: Calculate points dynamically
        var pointsAwarded = round.AwardPoints(player.Id, pointsToAward);
        if (!pointsAwarded)
            logger.LogWarning("Zero points were awarded to player with ID: {PlayerId}", player.Id);

        await playerService.IncrementPlayerScoreAsync(player.Id,
            pointsToAward); // TODO: Write to database after round end

        // end round if all players have been awarded points
        if (round.AllPlayersGuessed)
            round.EndRound(RoundEndReason.AllPlayersGuessed);

        return new GuessProcessedResult { IsCorrect = isCorrect, PointsAwarded = pointsToAward };
    }

    private StateSnapshot GetGameState() => new()
    {
        RoundActive = stateManager.SessionState == SessionState.RoundActive,
        RoundNumber = stateManager.Round?.Number ?? default,
        RoundId = stateManager.Round?.Id ?? default,
        IntervalStart = stateManager.IntervalStart,
        IntervalEnd = stateManager.IntervalEnd,
        WordHint = stateManager.Round?.WordHint,
    };
}
