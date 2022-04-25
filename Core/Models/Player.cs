namespace OhMyWord.Core.Models;

public class Player : Entity
{
    /// <summary>
    /// Client side generated unique visitor ID.
    /// </summary>
    public string VisitorId { get; init; } = string.Empty;

    /// <summary>
    /// SignalR hub context connection ID.
    /// </summary>
    public string ConnectionId { get; init; } = string.Empty;

    /// <summary>
    /// Total points this player has ever scored.
    /// </summary>
    public long Score { get; init; }

    public override string GetPartition() => Id;
}
