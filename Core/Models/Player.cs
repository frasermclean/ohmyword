namespace OhMyWord.Core.Models;

public class Player : Entity
{
    /// <summary>
    /// Client side generated unique visitor ID.
    /// </summary>
    public string VisitorId { get; init; } = string.Empty;

    /// <summary>
    /// Total points this player has ever scored.
    /// </summary>
    public long Score { get; init; }

    /// <summary>
    /// Number of times this player has registered with the game server.
    /// </summary>
    public int RegistrationCount { get; init; } = 1;

    public override string GetPartition() => Id;
}
