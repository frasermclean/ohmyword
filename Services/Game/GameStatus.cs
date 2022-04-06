namespace OhMyWord.Services.Game;

public record GameStatus
{
    public bool RoundActive { get; init; }
    public int RoundNumber { get; init; }
    public DateTime Expiry { get; init; }
}
