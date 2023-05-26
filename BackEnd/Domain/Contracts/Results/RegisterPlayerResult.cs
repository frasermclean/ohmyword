namespace OhMyWord.Domain.Contracts.Results;

public class RegisterPlayerResult
{
    public required bool IsSuccessful { get; init; }
    public required string PlayerId { get; init; }
    public required int PlayerCount { get; init; }
    public required long Score { get; init; }
    public required int RegistrationCount { get; init; }
    public required StateSnapshot StateSnapshot { get; init; }
}
