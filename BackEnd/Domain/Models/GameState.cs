namespace OhMyWord.Domain.Models;

public struct GameState
{
    public required bool RoundActive { get; init; }
    public required int RoundNumber { get; init; }
    public required Guid RoundId { get; init; }
    public required DateTime IntervalStart { get; init; }
    public required DateTime IntervalEnd { get; init; }
    public required WordHint? WordHint { get; init; }
}
