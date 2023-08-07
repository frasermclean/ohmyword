using FluentResults;
using System.Collections.Concurrent;

namespace OhMyWord.Core.Models;

public sealed class Round : IDisposable
{
    private readonly ConcurrentDictionary<Guid, RoundPlayerData> playerData;
    private readonly CancellationTokenSource cancellationTokenSource = new();

    public Round(Word word, TimeSpan letterHintDelay, IEnumerable<Guid>? playerIds = default)
    {
        Word = word;
        WordHint = new WordHint(word);
        EndDate = StartDate + word.Length * letterHintDelay;

        playerData = playerIds is null
            ? new ConcurrentDictionary<Guid, RoundPlayerData>()
            : new ConcurrentDictionary<Guid, RoundPlayerData>(playerIds.Select(playerId =>
                new KeyValuePair<Guid, RoundPlayerData>(playerId, new RoundPlayerData(playerId))));
    }

    public Guid Id { get; } = Guid.NewGuid();
    public required int Number { get; init; }
    public Word Word { get; }
    public WordHint WordHint { get; }
    public required int GuessLimit { get; init; }
    public DateTime StartDate { get; } = DateTime.UtcNow;
    public DateTime EndDate { get; }
    public RoundEndReason? EndReason { get; private set; }
    public required Guid SessionId { get; init; }
    public CancellationToken CancellationToken => cancellationTokenSource.Token;
    public int PlayerCount => playerData.Count;
    public bool AllPlayersGuessed => !playerData.IsEmpty && playerData.Values.All(player => player.PointsAwarded > 0);

    public bool IsActive => !cancellationTokenSource.IsCancellationRequested &&
                            DateTime.UtcNow > StartDate &&
                            DateTime.UtcNow < EndDate;

    public bool AddPlayer(Guid playerId)
        => playerData.TryAdd(playerId, new RoundPlayerData(playerId));

    public void EndRound(RoundEndReason endReason)
    {
        EndReason = endReason;
        cancellationTokenSource.Cancel();
    }

    public Result<int> ProcessGuess(Guid playerId, Guid roundId, string value)
    {
        // TODO: Move Round methods to RoundState service
        // check that round is active and ID matches
        if (!IsActive || roundId != Id)
            return Result.Fail("Round is inactive or ID does not match");

        // ensure player is in round
        if (!playerData.ContainsKey(playerId))
            return Result.Fail($"Player with ID: {playerId} is not in round");

        // check that player has not exceeded guess limit
        if (!IncrementGuessCount(playerId))
            return Result.Fail($"Guess limit: {GuessLimit} exceeded for player with ID: {playerId}");

        // compare guess value to word
        if (!string.Equals(value, Word.Id, StringComparison.InvariantCultureIgnoreCase))
            return Result.Fail($"Guess value of '{value}' is incorrect");

        // successful guess - award points
        var pointsAwarded = AwardPoints(playerId);
        return pointsAwarded;
    }

    public IEnumerable<RoundPlayerData> GetPlayerData() => playerData.Values;

    public void Dispose()
    {
        cancellationTokenSource.Dispose();
    }

    private bool IncrementGuessCount(Guid playerId)
    {
        var data = playerData[playerId];

        if (data.GuessCount >= GuessLimit)
            return false;

        data.GuessCount++;
        return true;
    }

    private int AwardPoints(Guid playerId)
    {
        var data = playerData[playerId];

        const int pointsToAward = 100; // TODO: Calculate points dynamically

        data.PointsAwarded = pointsToAward;
        data.GuessTime = DateTime.UtcNow - StartDate;

        return pointsToAward;
    }

    public static readonly Round Default = new(Word.Default, TimeSpan.Zero)
    {
        Number = 0, GuessLimit = 0, SessionId = Guid.Empty
    };
}
