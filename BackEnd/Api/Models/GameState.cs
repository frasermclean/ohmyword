namespace OhMyWord.Api.Models;

public record GameState
{
    public bool RoundActive { get; init; }
    public int RoundNumber { get; init; }
    public Guid RoundId { get; init; }
    public DateTime IntervalStart { get; init; } = DateTime.UtcNow;
    public DateTime IntervalEnd { get; init; }
    public WordHint WordHint { get; init; } = WordHint.Default;
}
