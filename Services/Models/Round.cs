using System.Text.Json.Serialization;
using OhMyWord.Core.Models;

namespace OhMyWord.Services.Models;

public class Round : IDisposable
{
    private readonly CancellationTokenSource cancellationTokenSource = new();

    public Guid Id { get; } = Guid.NewGuid();
    public int Number { get; }
    public Word Word { get; }
    public WordHint WordHint { get; }
    public TimeSpan Duration { get; }
    public DateTime Expiry { get; }
    public RoundEndReason EndReason { get; private set; } = RoundEndReason.Timeout;

    [JsonIgnore]
    public CancellationToken CancellationToken => cancellationTokenSource.Token;

    internal Round(int number, Word word, TimeSpan duration)
    {
        Number = number;
        Word = word;
        WordHint = new WordHint(word);
        Duration = duration;
        Expiry = DateTime.UtcNow + duration;
    }

    /// <summary>
    /// End the round early.
    /// </summary>
    internal void EndRound(RoundEndReason reason)
    {
        EndReason = reason;
        cancellationTokenSource.Cancel();
    }

    public void Dispose()
    {
        cancellationTokenSource.Dispose();
        GC.SuppressFinalize(this);
    }
}
