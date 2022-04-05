using OhMyWord.Core.Models;

namespace OhMyWord.Services.Game;

public class GameStatus
{
    public bool RoundActive { get; init; }
    public DateTime Expiry { get; init; }
    public int PlayerCount { get; init; }
}
