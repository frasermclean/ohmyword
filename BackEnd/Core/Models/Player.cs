using System.Net;

namespace OhMyWord.Core.Models;

public record Player
{
    /// <summary>
    /// Unique identifier for this player.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Public name for this player.
    /// </summary>
    public string? Name { get; init; }

    /// <summary>
    /// SignalR connection ID.
    /// </summary>
    public string? ConnectionId { get; init; }

    /// <summary>
    /// Fingerprint of this player's browser.
    /// </summary>
    public string? VisitorId { get; init; }

    /// <summary>
    /// User ID if this player has signed in.
    /// </summary>
    public required Guid? UserId { get; init; }

    /// <summary>
    /// Total points this player has ever scored.
    /// </summary>
    public required long Score { get; init; }

    /// <summary>
    /// Number of times this player has registered with the game server.
    /// </summary>
    public required int RegistrationCount { get; init; }

    /// <summary>
    /// Player's current IP address.
    /// </summary>
    public IPAddress? IpAddress { get; init; }

    /// <summary>
    /// The player's current geo location.
    /// </summary>
    public GeoLocation? GeoLocation { get; init; }
}
