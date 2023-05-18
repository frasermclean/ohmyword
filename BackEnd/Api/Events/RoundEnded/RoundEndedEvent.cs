using OhMyWord.Api.Models;
using OhMyWord.Domain.Models;
using OhMyWord.Infrastructure.Models.Entities;

namespace OhMyWord.Api.Events.RoundEnded;

public class RoundEndedEvent : IEvent
{
    public required Guid RoundId { get; init; }
    public required string Word { get; init; }
    public required RoundEndReason EndReason { get; init; }
    public required DateTime NextRoundStart { get; init; }
    public required IEnumerable<ScoreLine> Scores { get; init; }
}
