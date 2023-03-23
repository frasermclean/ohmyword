namespace OhMyWord.Infrastructure.Entities;

public record PlayerEntity : Entity
{
    /// <summary>
    /// User ID if this player has a registered account.
    /// </summary>
    public string? UserId { get; init; }

    /// <summary>
    /// Total points this player has ever scored.
    /// </summary>
    public long Score { get; init; }

    /// <summary>
    /// Number of times this player has registered with the game server.
    /// </summary>
    public int RegistrationCount { get; init; } = 1;
}
