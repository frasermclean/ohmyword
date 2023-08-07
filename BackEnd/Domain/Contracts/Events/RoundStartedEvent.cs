using FastEndpoints;
using OhMyWord.Core.Models;

namespace OhMyWord.Domain.Contracts.Events;

public class RoundStartedEvent : IEvent
{
    public required int RoundNumber { get; init; }
    public required Guid RoundId { get; init; }
    public required WordHint WordHint { get; init; }
    public required DateTime StartDate { get; init; }
    public required DateTime EndDate { get; init; }
}
