using OhMyWord.Logic.Models;

namespace OhMyWord.Api.Models;

public class RegisterPlayerResponse
{
    public required bool IsSuccessful { get; init; }
    public required Guid PlayerId { get; init; }
    public required int PlayerCount { get; init; }
    public required long Score { get; init; }
    public required int RegistrationCount { get; init; }
    public required RoundStateSnapshotResponse StateSnapshot { get; init; }

    public static RegisterPlayerResponse FromRegisterPlayerResult(RegisterPlayerResult result) => new()
    {
        IsSuccessful = result.IsSuccessful,
        PlayerId = result.PlayerId,
        PlayerCount = result.PlayerCount,
        Score = result.Score,
        RegistrationCount = result.RegistrationCount,
        StateSnapshot = RoundStateSnapshotResponse.FromRoundStateSnapshot(result.RoundStateSnapshot)
    };
}
