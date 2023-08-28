using FluentResults;
using Microsoft.Extensions.Logging;
using OhMyWord.Core.Models;
using OhMyWord.Logic.Models;

namespace OhMyWord.Logic.Services.State;

public interface IRoundState : IState
{
    bool IsActive { get; }
    int RoundNumber { get; }
    Guid RoundId { get; }
    Interval Interval { get; }

    Task<RoundStartData> CreateRoundAsync(CancellationToken cancellationToken = default);
    Task<RoundSummary> ExecuteRoundAsync(CancellationToken cancellationToken = default);

    void EndRound(RoundEndReason endReason);
    void AddPlayer(Guid playerId);
    Result<int> ProcessGuess(Guid playerId, Guid roundId, string value);
    RoundStateSnapshot GetSnapshot();
}

public class RoundState : IRoundState
{
    private readonly ILogger<RoundState> logger;
    private readonly ISessionState sessionState;
    private readonly IRoundService roundService;


    private Round round = Round.Default;
    private CancellationTokenSource? roundCancellationTokenSource;

    public RoundState(ILogger<RoundState> logger, ISessionState sessionState, IRoundService roundService)
    {
        this.logger = logger;
        this.sessionState = sessionState;
        this.roundService = roundService;
    }

    public bool IsActive => round != Round.Default && round.EndDate > DateTime.UtcNow;
    public Guid RoundId => round.Id;
    public int RoundNumber => round.Number;
    public Interval Interval { get; private set; } = Interval.Default;
    public bool IsDefault => round == Round.Default && Interval == Interval.Default;

    public async Task<RoundStartData> CreateRoundAsync(CancellationToken cancellationToken)
    {
        round = await roundService.CreateRoundAsync(sessionState.IncrementRoundCount(), sessionState.SessionId,
            cancellationToken: cancellationToken);
        Interval = new Interval(round.StartDate, round.EndDate);

        return new RoundStartData
        {
            RoundNumber = round.Number,
            RoundId = round.Id,
            WordHint = round.WordHint,
            StartDate = round.StartDate,
            EndDate = round.EndDate
        };
    }

    public async Task<RoundSummary> ExecuteRoundAsync(
        CancellationToken cancellationToken)
    {
        using (roundCancellationTokenSource = new CancellationTokenSource())
        {
            // create linked cancellation token source
            using var linkedTokenSource =
                CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, roundCancellationTokenSource.Token);

            // execute round
            var summary = await roundService.ExecuteRoundAsync(round, linkedTokenSource.Token);
            if (round.EndReason is null)
                EndRound(RoundEndReason.Timeout);

            return summary;
        }
    }

    public void EndRound(RoundEndReason endReason)
    {
        if (!IsActive)
            return;

        logger.LogInformation("Ending round {RoundId}, reason: {EndReason}", round.Id, endReason);
        round.EndReason = endReason;
        round.EndDate = DateTime.UtcNow;
        round = Round.Default;

        // cancel round in progress
        if (!roundCancellationTokenSource?.IsCancellationRequested ?? false)
        {
            roundCancellationTokenSource.Cancel();
        }
    }

    public void AddPlayer(Guid playerId)
    {
        if (!IsActive)
        {
            logger.LogWarning("AddPlayer called when round is not active");
            return;
        }

        logger.LogInformation("Adding player: {PlayerId} to round: {RoundId}", playerId, round.Id);
        round.PlayerData[playerId] = new RoundPlayerData();
    }

    public Result<int> ProcessGuess(Guid playerId, Guid roundId, string value)
    {
        // check that round is active and ID matches
        if (!IsActive || roundId != RoundId)
            return Result.Fail("Round is inactive or ID does not match");

        // ensure player is in round
        if (!round.PlayerData.TryGetValue(playerId, out var data))
            return Result.Fail($"Player with ID: {playerId} is not in round");

        // check that player has not exceeded guess limit
        if (!IsUnderGuessLimit())
            return Result.Fail($"Guess limit: {round.GuessLimit} exceeded for player with ID: {playerId}");

        // compare guess value to word
        if (!string.Equals(value, round.Word.Id, StringComparison.InvariantCultureIgnoreCase))
            return Result.Fail($"Guess value of '{value}' is incorrect");

        // successful guess - award points
        var points = AwardPoints();

        // end the round if all players have guessed
        if (round.AllPlayersAwarded)
            EndRound(RoundEndReason.AllPlayersGuessed);

        return points;

        bool IsUnderGuessLimit()
        {
            if (data.GuessCount >= round.GuessLimit)
                return false;

            data.GuessCount++;
            return true;
        }

        int AwardPoints()
        {
            const int pointsToAward = 100; // TODO: Calculate points dynamically

            data.PointsAwarded = pointsToAward;
            data.GuessTime = DateTime.UtcNow - round.StartDate;

            return pointsToAward;
        }
    }

    public RoundStateSnapshot GetSnapshot() => new()
    {
        RoundActive = IsActive,
        RoundNumber = round.Number,
        RoundId = round.Id,
        Interval = Interval,
        WordHint = round.WordHint,
    };

    public void Reset()
    {
        round = Round.Default;
        Interval = Interval.Default;
    }
}
