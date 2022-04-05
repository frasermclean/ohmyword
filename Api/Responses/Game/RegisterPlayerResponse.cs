using OhMyWord.Services.Game;

namespace OhMyWord.Api.Responses.Game;

public class RegisterPlayerResponse
{
    public bool Success { get; init; } = true;
    public string PlayerId { get; init; } = string.Empty;
    public GameStatus Status { get; init; } = new();
}
