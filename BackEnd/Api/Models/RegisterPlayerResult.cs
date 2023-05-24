using OhMyWord.Domain.Models;

namespace OhMyWord.Api.Models;

public class RegisterPlayerResult
{
    public required int PlayerCount { get; init; }
    public required long Score { get; init; }
    public required GameState GameState { get; init; }
}
