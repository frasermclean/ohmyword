namespace OhMyWord.Api.Models;

public class PlayerRegisteredResult
{
    public required int PlayerCount { get; init; }
    public required long Score { get; init; }
    public required StateSnapshot StateSnapshot { get; init; }
}
