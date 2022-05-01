using OhMyWord.Core.Models;

namespace OhMyWord.Services.Events;

public class RoundStartedEventArgs : EventArgs
{
    public Guid RoundId { get; init; }
    public int RoundNumber { get; init; }
    public DateTime RoundEnds { get; init; }
    public WordHint WordHint { get; init; } = WordHint.Default;
}
