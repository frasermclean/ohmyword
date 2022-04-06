using OhMyWord.Core.Models;
using OhMyWord.Services.Game;

namespace OhMyWord.Api.Responses.Game;

public class RegisterPlayerResponse
{
    public string PlayerId { get; init; } = string.Empty;
    public GameStatus GameStatus { get; init; } = new();
    public WordHint WordHint { get; init; } = WordHint.Default;
    public int PlayerCount { get; init; }
}
