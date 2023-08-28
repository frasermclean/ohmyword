using OhMyWord.Core.Models;

namespace OhMyWord.Logic.Extensions;

public static class RoundEntityExtensions
{
    public static RoundEntity ToEntity(this Round round) => new()
    {
        Id = round.Id.ToString(),
        Number = round.Number,
        WordId = round.Word.Id,
        DefinitionId = round.WordHint.Definition.Id,
        GuessLimit = round.GuessLimit,
        StartDate = round.StartDate,
        EndDate = round.EndDate,
        EndReason = round.EndReason ?? throw new InvalidOperationException("Round end reason has not been set"),
        SessionId = round.SessionId,
        PlayerData = round.PlayerData.Values
    };
}
