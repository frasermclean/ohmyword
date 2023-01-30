using OhMyWord.Data.Enums;

namespace OhMyWord.Core.Responses.Game;

public class RoundEndResponse
{
    public int RoundNumber { get; init; }
    public Guid RoundId { get; init; }
    public string WordId { get; init; } = string.Empty;
    public RoundEndReason EndReason { get; init; }
    public DateTime NextRoundStart { get; init; }
}
