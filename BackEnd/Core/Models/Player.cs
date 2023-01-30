namespace OhMyWord.Core.Models;

public record Player
{
    public required Guid Id { get; init; }

    /// <summary>
    /// Client side generated unique visitor ID.
    /// </summary>
    public required string VisitorId { get; init; }

    /// <summary>
    /// Total points this player has ever scored.
    /// </summary>
    public required long Score { get; init; }

    /// <summary>
    /// Number of times this player has registered with the game server.
    /// </summary>
    public required int RegistrationCount { get; init; }
}
