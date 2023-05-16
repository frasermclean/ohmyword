using OhMyWord.Infrastructure.Models.Entities;

namespace OhMyWord.Domain.Models;

public struct RoundEndData
{
    public required Guid RoundId { get; init; }
    public required string Word { get; init; }
    public required RoundEndReason EndReason { get; init; }
    public required TimeSpan PostRoundDelay { get; init; }
    public required DateTime NextRoundStart { get; init; }
}
