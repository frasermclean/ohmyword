using OhMyWord.Data.Enums;
using System.Collections.Concurrent;
using System.Text.Json.Serialization;

namespace OhMyWord.Api.Models;

public class Round : IDisposable
{
    private readonly CancellationTokenSource cancellationTokenSource = new();
    private readonly ConcurrentDictionary<string, RoundVisitorData> visitors = new();

    public Guid Id { get; } = Guid.NewGuid();
    public int Number { get; }
    public Word Word { get; }
    public WordHint WordHint { get; }
    public DateTime StartTime { get; }
    public TimeSpan Duration { get; }
    public DateTime EndTime => StartTime + Duration;
    public RoundEndReason EndReason { get; private set; } = RoundEndReason.Timeout;
    public int VisitorCount => visitors.Count;
    public bool AllVisitorsAwarded => visitors.Values.Count(data => data.PointsAwarded > 0) == VisitorCount;

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
            AddVisitor(visitorId);
    }

    public bool AddVisitor(string visitor) => visitors.TryAdd(visitor, new RoundVisitorData());
    public bool RemoveVisitor(string visitorId) => visitors.TryRemove(visitorId, out _);

    public bool IncrementGuessCount(string visitorId)
    {
        if (!visitors.TryGetValue(visitorId, out var data))
            return false;

        data.GuessCount++;
        return true;
    }

    public int AwardVisitor(string visitorId)
    {
        if (!visitors.TryGetValue(visitorId, out var data))
            return 0;

        const int points = 100; // TODO: Calculate points dynamically
        data.PointsAwarded = points;

        return points;
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
