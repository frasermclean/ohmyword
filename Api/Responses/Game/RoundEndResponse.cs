using OhMyWord.Core.Models;

namespace OhMyWord.Api.Responses.Game;

public class RoundEndResponse
{
    public int RoundNumber { get; init; }
    public Guid RoundId { get; init; }
    public string Word { get; init; } = string.Empty;
    public RoundEndReason EndReason { get; init; }
    public DateTime NextRoundStart { get; init; }
}
