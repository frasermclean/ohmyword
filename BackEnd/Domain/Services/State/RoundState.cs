using FastEndpoints;
using FluentResults;
using Microsoft.Extensions.Logging;
using OhMyWord.Core.Models;
using OhMyWord.Domain.Contracts.Events;

namespace OhMyWord.Domain.Services.State;

public interface IRoundState : IState
{
    bool IsActive { get; }
    int RoundNumber { get; }
    Guid RoundId { get; }
    Interval Interval { get; }

    Task<RoundSummary> ExecuteRoundAsync(CancellationToken cancellationToken = default);

    void EndRound(RoundEndReason endReason);
    void AddPlayer(Guid playerId);
    Result<int> ProcessGuess(Guid playerId, Guid roundId, string value);
    StateSnapshot GetSnapshot();
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

    public bool IsActive => round != Round.Default && round.IsActive;
    public Guid RoundId => round.Id;
    public int RoundNumber => round.Number;
    public Interval Interval { get; private set; } = Interval.Default;
    public bool IsDefault => round == Round.Default && Interval == Interval.Default;

    public async Task<RoundSummary> ExecuteRoundAsync(
        CancellationToken cancellationToken)
    {
        // create new round
        round = await roundService.CreateRoundAsync(sessionState.IncrementRoundCount(), sessionState.SessionId,
            cancellationToken: cancellationToken);
        Interval = new Interval(round.StartDate, round.EndDate);
        roundCancellationTokenSource = new CancellationTokenSource();

        // send round started notification
        await SendRoundStartedNotificationAsync(round, cancellationToken);

        // create linked cancellation token source
        using var linkedTokenSource =
            CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, roundCancellationTokenSource.Token);

        // execute round
        var summary = await roundService.ExecuteRoundAsync(round, linkedTokenSource.Token);

        if (round.EndReason is null)
            EndRound(RoundEndReason.Timeout);
        await SendRoundEndedNotificationAsync(summary, cancellationToken);

        roundCancellationTokenSource.Dispose();

        return summary;
    }

    public void EndRound(RoundEndReason endReason)
    {
        if (!IsActive)
            return;

        logger.LogInformation("Ending round {RoundId}, reason: {EndReason}", round.Id, endReason);
        round.EndReason = endReason;
        round.EndDate = DateTime.UtcNow;

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
        round.PlayerData[playerId] = new RoundPlayerData(playerId);
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
        if (round.AllPlayersGuessed)
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

    public StateSnapshot GetSnapshot() => new()
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

    private static Task SendRoundEndedNotificationAsync(RoundSummary summary, CancellationToken cancellationToken)
        => new RoundEndedEvent(summary).PublishAsync(cancellation: cancellationToken);

    private static Task SendRoundStartedNotificationAsync(Round round, CancellationToken cancellationToken)
        => new RoundStartedEvent
        {
            RoundNumber = round.Number,
            RoundId = round.Id,
            WordHint = round.WordHint,
            StartDate = round.StartDate,
            EndDate = round.EndDate
        }.PublishAsync(cancellation: cancellationToken);
}
