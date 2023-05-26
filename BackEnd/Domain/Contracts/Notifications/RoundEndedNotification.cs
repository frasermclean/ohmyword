using MediatR;
using OhMyWord.Domain.Models;
using OhMyWord.Infrastructure.Models.Entities;

namespace OhMyWord.Domain.Contracts.Notifications;

public class RoundEndedNotification : INotification
{
    public required string Word { get; init; }
    public required RoundEndReason EndReason { get; init; }
    public required Guid RoundId { get; init; }
    public required Guid DefinitionId { get; init; }
    public required DateTime NextRoundStart { get; init; }
    public required IEnumerable<ScoreLine> Scores { get; init; }
}
