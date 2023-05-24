using OhMyWord.Api.Models;
using OhMyWord.Domain.Models;
using OhMyWord.Domain.Services;
using OhMyWord.Infrastructure.Models.Entities;
using System.Net;

namespace OhMyWord.Api.Services;

public interface IGameService
{
    Task ExecuteGameLoopAsync(CancellationToken cancellationToken);
    void RemovePlayer(string playerId);

    Task<RegisterPlayerResult> RegisterPlayerAsync(string connectionId, string visitorId, IPAddress ipAddress,
        Guid? userId, CancellationToken cancellationToken = default);

    Task<ProcessGuessResult> ProcessGuessAsync(string connectionId, Guid roundId, string value);
}

public class GameService : IGameService
{
    private readonly ILogger<GameService> logger;
    private readonly IServiceProvider serviceProvider;
    private readonly IPlayerService playerService;

    private Round Round { get; set; } = Round.Default;
    private bool IsRoundActive => Round != Round.Default;

    public GameService(ILogger<GameService> logger, IServiceProvider serviceProvider, IPlayerService playerService)
    {
        this.logger = logger;
        this.serviceProvider = serviceProvider;
        this.playerService = playerService;
    }

    public async Task ExecuteGameLoopAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            // sleep while there are no players
            if (playerService.PlayerCount == 0)
            {
                await Task.Delay(1000, cancellationToken);
                continue;
            }

            using var scope = serviceProvider.CreateScope();
            var sessionManager = scope.ServiceProvider.GetRequiredService<ISessionManager>();
            await sessionManager.ExecuteAsync(cancellationToken);
        }
    }

    public async Task<RegisterPlayerResult> RegisterPlayerAsync(string connectionId, string visitorId,
        IPAddress ipAddress, Guid? userId, CancellationToken cancellationToken)
    {
        var player = await playerService.AddPlayerAsync(visitorId, connectionId, ipAddress, userId);

        AddPlayer(player.Id); // TODO: Move to SessionManager

        return new RegisterPlayerResult
        {
            PlayerCount = playerService.PlayerCount, Score = player.Score, GameState = GetGameState()
        };
    }

    public async Task<ProcessGuessResult> ProcessGuessAsync(string connectionId, Guid roundId, string value)
    {
        // validate round state
        if (!IsRoundActive || roundId != Round.Id) return ProcessGuessResult.Default;

        // compare value to current word value
        var isCorrect = string.Equals(value, Round.Word.Id, StringComparison.InvariantCultureIgnoreCase);
        if (!isCorrect) return ProcessGuessResult.Default;

        var player = playerService.GetPlayerByConnectionId(connectionId);
        if (player is null)
        {
            logger.LogError("Player not found. ConnectionId: {ConnectionId}", connectionId);
            return ProcessGuessResult.Default;
        }

        Round.IncrementGuessCount(player.Id);

        const int pointsToAward = 100; // TODO: Calculate points dynamically
        var pointsAwarded = Round.AwardPoints(player.Id, pointsToAward);
        if (!pointsAwarded)
            logger.LogWarning("Zero points were awarded to player with ID: {PlayerId}", player.Id);

        await playerService.IncrementPlayerScoreAsync(player.Id,
            pointsToAward); // TODO: Write to database after round end

        // end round if all players have been awarded points
        if (Round.AllPlayersGuessed)
            Round.EndRound(RoundEndReason.AllPlayersGuessed);

        return new ProcessGuessResult { IsCorrect = isCorrect, PointsAwarded = pointsToAward };
    }

    public void AddPlayer(string playerId)
    {
        if (IsRoundActive)
            Round.AddPlayer(playerId);
    }

    public void RemovePlayer(string playerId)
    {
        // player left while round wasn't active
        if (!IsRoundActive)
            return;

        // last visitor left while round active
        if (playerService.PlayerCount == 0)
            Round.EndRound(RoundEndReason.NoPlayersLeft);
    }

    private GameState GetGameState() => new()
    {
        RoundActive = Round != Round.Default,
        RoundNumber = Round.Number,
        RoundId = Round.Id,
        IntervalStart = IsRoundActive ? Round.StartDate : default,
        IntervalEnd = IsRoundActive ? Round.EndDate : default,
        WordHint = IsRoundActive ? Round.WordHint : default,
    };
}
