using OhMyWord.Core.Models;
using OhMyWord.Logic.Models;

namespace OhMyWord.Api.Models;

public class RoundEndedResponse
{
    public required string Word { get; init; }
    public required PartOfSpeech PartOfSpeech { get; init; }
    public required RoundEndReason EndReason { get; init; }
    public required Guid RoundId { get; init; }
    public required Guid DefinitionId { get; init; }
    public required DateTime NextRoundStart { get; init; }
    public required IEnumerable<ScoreLine> Scores { get; init; }

    public static RoundEndedResponse FromRoundSummary(RoundSummary summary) => new()
    {
        Word = summary.Word,
        PartOfSpeech = summary.PartOfSpeech,
        EndReason = summary.EndReason,
        RoundId = summary.RoundId,
        DefinitionId = summary.DefinitionId,
        NextRoundStart = summary.NextRoundStart,
        Scores = summary.Scores
    };
}
