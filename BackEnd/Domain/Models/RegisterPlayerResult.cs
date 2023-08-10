namespace OhMyWord.Domain.Models;

public class RegisterPlayerResult
{
    public required bool IsSuccessful { get; init; }
    public required Guid PlayerId { get; init; }
    public required int PlayerCount { get; init; }
    public required long Score { get; init; }
    public required int RegistrationCount { get; init; }
    public required RoundStateSnapshot RoundStateSnapshot { get; init; }
}
