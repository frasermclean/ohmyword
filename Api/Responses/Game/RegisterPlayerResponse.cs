using OhMyWord.Core.Models;
using OhMyWord.Services.Game;

namespace OhMyWord.Api.Responses.Game;

public class RegisterPlayerResponse
{
    public string PlayerId { get; init; } = string.Empty;
    public int RoundNumber { get; init; }
    public bool RoundActive { get; init; }
    public WordHint? WordHint { get; init; }
    public int PlayerCount { get; init; }
    public DateTime Expiry { get; init; }
}
