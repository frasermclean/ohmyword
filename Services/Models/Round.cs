using OhMyWord.Core.Models;
using System.Collections.Concurrent;
using System.Text.Json.Serialization;

namespace OhMyWord.Services.Models;

public class Round : IDisposable
{
    private readonly CancellationTokenSource cancellationTokenSource = new();
    private readonly ConcurrentDictionary<string, RoundPlayerData> players = new();

    public Guid Id { get; } = Guid.NewGuid();
    public int Number { get; }
    public Word Word { get; }
    public WordHint WordHint { get; }
    public DateTime StartTime { get; }
    public TimeSpan Duration { get; }
    public DateTime EndTime => StartTime + Duration;
    public RoundEndReason EndReason { get; private set; } = RoundEndReason.Timeout;

    [JsonIgnore]
    public CancellationToken CancellationToken => cancellationTokenSource.Token;

    public Round(int number, Word word, TimeSpan duration, IEnumerable<string>? playerIds = default)
    {
        Number = number;
        Word = word;
        WordHint = new WordHint(word);
        StartTime = DateTime.UtcNow;
        Duration = duration;

        if (playerIds is null) return;

        foreach (var playerId in playerIds)
            AddPlayer(playerId);
    }

    public bool AddPlayer(string playerId) => players.TryAdd(playerId, new RoundPlayerData());
    public bool RemovePlayer(string playerId) => players.TryRemove(playerId, out _);

    public bool IncrementGuessCount(string playerId)
    {
        if (!players.TryGetValue(playerId, out var playerData))
            return false;

        playerData.GuessCount++;
        return true;
    }

    public int AwardPlayer(string playerId)
    {
        if (!players.TryGetValue(playerId, out var playerData))
            return 0;

        const int points = 100; // TODO: Calculate points dynamically
        playerData.PointsAwarded = points;

        return points;
    }

    public bool AllPlayersAwarded => players.Values.Count(data => data.PointsAwarded > 0) == players.Count;

    /// <summary>
    /// End the round early.
    /// </summary>
    public void EndRound(RoundEndReason reason)
    {
        EndReason = reason;
        cancellationTokenSource.Cancel();
    }

    public void Dispose()
    {
        cancellationTokenSource.Dispose();
        GC.SuppressFinalize(this);
    }

    public static readonly Round Default = new(0, Word.Default, TimeSpan.FromSeconds(10));
}
