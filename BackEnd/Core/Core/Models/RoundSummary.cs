namespace OhMyWord.Core.Models;

public class RoundSummary
{
    public required string Word { get; init; }
    public required PartOfSpeech PartOfSpeech { get; init; }
    public required RoundEndReason EndReason { get; init; }
    public required Guid RoundId { get; init; }
    public required Guid DefinitionId { get; init; }
    public required DateTime NextRoundStart { get; init; }
    public required IEnumerable<ScoreLine> Scores { get; init; }
}
