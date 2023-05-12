using OhMyWord.Infrastructure.Models;
using System.Collections.Concurrent;
using System.Text.Json.Serialization;

namespace OhMyWord.Domain.Models;

public class Round : IDisposable
{
    private readonly CancellationTokenSource cancellationTokenSource = new();
    private readonly ConcurrentBag<RoundPlayerData> playerData = new();

    public Guid Id { get; } = Guid.NewGuid();
    public int Number { get; }
    public Word Word { get; }
    public WordHint WordHint { get; }
    public DateTime StartTime { get; }
    public TimeSpan Duration { get; }
    public DateTime EndTime => StartTime + Duration;
    public RoundEndReason EndReason { get; private set; } = RoundEndReason.Timeout;
    public int PlayerCount => playerData.Count;
    public bool AllPlayersAwarded => playerData.All(data => data.GuessedCorrectly);

    [JsonIgnore] public CancellationToken CancellationToken => cancellationTokenSource.Token;

    public Round(int number, Word word, TimeSpan duration, IEnumerable<string>? visitorIds = default)
    {
        Number = number;
        Word = word;
        WordHint = new WordHint(word);
        StartTime = DateTime.UtcNow;
        Duration = duration;

        if (visitorIds is null) return;

        foreach (var visitorId in visitorIds)
            AddPlayer(visitorId);
    }

    public void AddPlayer(string playerId) => playerData.Add(new RoundPlayerData { PlayerId = playerId });

    public bool IncrementGuessCount(string playerId)
    {
        var data = playerData.FirstOrDefault(d => d.PlayerId == playerId);
        if (data is null) return false;

        data.GuessCount++;
        return true;
    }

    public bool AwardVisitor(string playerId, int points)
    {
        var data = playerData.FirstOrDefault(d => d.PlayerId == playerId);
        if (data is null) return false;

        data.PointsAwarded = points;
        data.GuessedCorrectly = true;

        return true;
    }

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
