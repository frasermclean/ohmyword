namespace OhMyWord.Domain.Models;

public record Visitor
{
    /// <summary>
    /// Client side generated unique visitor ID.
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// Total points this visitor has ever scored.
    /// </summary>
    public required long Score { get; init; }

    /// <summary>
    /// Number of times this visitor has registered with the game server.
    /// </summary>
    public required int RegistrationCount { get; init; }
}
