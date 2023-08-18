using OhMyWord.Core.Models;
using OhMyWord.Domain.Models;

namespace OhMyWord.Api.Models;

public class RoundStateSnapshotResponse
{
    public required bool RoundActive { get; init; }
    public required int RoundNumber { get; set; }
    public required Guid RoundId { get; set; }
    public required Interval Interval { get; set; }
    public required WordHintResponse WordHint { get; init; }

    public static RoundStateSnapshotResponse FromRoundStateSnapshot(RoundStateSnapshot snapshot) => new()
    {
        RoundActive = snapshot.RoundActive,
        RoundNumber = snapshot.RoundNumber,
        RoundId = snapshot.RoundId,
        Interval = snapshot.Interval,
        WordHint = WordHintResponse.FromWordHint(snapshot.WordHint)
    };
}
