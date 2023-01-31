namespace OhMyWord.Api.Models;

public record GameState
{
    public bool RoundActive { get; init; }
    public int RoundNumber { get; init; }
    public Guid RoundId { get; init; }
    public DateTime Expiration { get; init; }    
}
