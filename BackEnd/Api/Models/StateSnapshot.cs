using OhMyWord.Domain.Models;

namespace OhMyWord.Api.Models;

public struct StateSnapshot
{
    public required bool RoundActive { get; init; }
    public required int RoundNumber { get; init; }
    public required Guid RoundId { get; init; }
    public required DateTime IntervalStart { get; init; }
    public required DateTime IntervalEnd { get; init; }
    public required WordHint? WordHint { get; init; }
}
