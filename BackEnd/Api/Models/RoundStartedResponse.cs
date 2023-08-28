using OhMyWord.Logic.Models;

namespace OhMyWord.Api.Models;

public class RoundStartedResponse
{
    public required int RoundNumber { get; init; }
    public required Guid RoundId { get; init; }
    public required WordHintResponse WordHint { get; init; }
    public required DateTime StartDate { get; init; }
    public required DateTime EndDate { get; init; }

    public static RoundStartedResponse FromRoundStartData(RoundStartData data) => new()
    {
        RoundNumber = data.RoundNumber,
        RoundId = data.RoundId,
        WordHint = WordHintResponse.FromWordHint(data.WordHint),
        StartDate = data.StartDate,
        EndDate = data.EndDate
    };
}
