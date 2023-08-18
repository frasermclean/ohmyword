using OhMyWord.Core.Models;

namespace OhMyWord.Domain.Models;

public struct RoundStateSnapshot
{
    public required bool RoundActive { get; init; }
    public required int RoundNumber { get; init; }
    public required Guid RoundId { get; init; }
    public required Interval Interval { get; init; }
    public required WordHint WordHint { get; init; }
}
