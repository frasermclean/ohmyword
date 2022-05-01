using OhMyWord.Core.Models;
using System.Text.Json.Serialization;

namespace OhMyWord.Services.Models;

public class Round : IDisposable
{
    private readonly CancellationTokenSource cancellationTokenSource = new();

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

    public Round(int number, Word word, TimeSpan duration)
    {
        Number = number;
        Word = word;
        WordHint = new WordHint(word);
        StartTime = DateTime.UtcNow;
        Duration = duration;
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
