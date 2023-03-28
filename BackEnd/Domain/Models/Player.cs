﻿namespace OhMyWord.Domain.Models;

public record Player
{
    /// <summary>
    /// Unique identifier for this player.
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// SignalR connection ID.
    /// </summary>
    public required string ConnectionId { get; init; }

    /// <summary>
    /// User ID if this player has signed in.
    /// </summary>
    public required string? UserId { get; init; }

    /// <summary>
    /// Total points this player has ever scored.
    /// </summary>
    public required long Score { get; init; }

    /// <summary>
    /// Number of times this player has registered with the game server.
    /// </summary>
    public required int RegistrationCount { get; init; }
}