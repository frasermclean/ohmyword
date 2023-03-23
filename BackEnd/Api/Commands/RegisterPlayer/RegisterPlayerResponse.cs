using OhMyWord.Domain.Models;

namespace OhMyWord.Api.Commands.RegisterPlayer;

public class RegisterPlayerResponse
{
    public required int PlayerCount { get; init; }
    public required long Score { get; init; }
    public required GameState GameState { get; init; }
}
