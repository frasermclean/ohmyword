namespace OhMyWord.Data.Entities;

public record VisitorEntity : Entity
{
    /// <summary>
    /// Total points this visitor has ever scored.
    /// </summary>
    public long Score { get; init; }

    /// <summary>
    /// Number of times this visitor has registered with the game server.
    /// </summary>
    public int RegistrationCount { get; init; } = 1;
}
