using FluentResults;
using Microsoft.Extensions.Logging;
using OhMyWord.Domain.Models;
using OhMyWord.Infrastructure.Models.Entities;

namespace OhMyWord.Domain.Services.State;

public interface IRoundState : IState
{
    bool IsActive { get; }
    int RoundNumber { get; }
    Guid RoundId { get; }
    Interval Interval { get; }

    Task<Round> NextRoundAsync(CancellationToken cancellationToken = default);
    Result<int> ProcessGuess(Guid playerId, Guid roundId, string value);
    StateSnapshot GetSnapshot();
}

public class RoundState : IRoundState
{
    private readonly ILogger<RoundState> logger;
    private readonly ISessionState sessionState;
    private readonly IRoundService roundService;

    private Round round = Round.Default;

    public RoundState(ILogger<RoundState> logger, ISessionState sessionState, IRoundService roundService)
    {
        this.logger = logger;
        this.sessionState = sessionState;
        this.roundService = roundService;
    }

    public bool IsActive => !IsDefault && round.IsActive;
    public int RoundNumber => round.Number;
    public Guid RoundId => round.Id;
    public Interval Interval { get; private set; } = Interval.Default;
    public bool IsDefault => round == Round.Default && Interval == Interval.Default;

    public async Task<Round> NextRoundAsync(CancellationToken cancellationToken)
    {
        var roundNumber = sessionState.IncrementRoundCount();
        round = await roundService.CreateRoundAsync(roundNumber, sessionState.SessionId, cancellationToken);
        logger.LogInformation("Created round with ID: {RoundId}", round.Id);

        Interval = new Interval(round.StartDate, round.EndDate);

        return round;
    }

    public Result<int> ProcessGuess(Guid playerId, Guid roundId, string value)
    {
        var result = round.ProcessGuess(playerId, roundId, value);

        // end the round if all players have guessed
        if (round.AllPlayersGuessed)
            round.EndRound(RoundEndReason.AllPlayersGuessed);

        return result;
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
}
