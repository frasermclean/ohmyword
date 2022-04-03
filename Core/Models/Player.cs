namespace OhMyWord.Core.Models;

public class Player : Entity
{
    /// <summary>
    /// Client side generated unique visitor ID.
    /// </summary>
    public string VisitorId { get; set; } = string.Empty;

    /// <summary>
    /// SignalR hub context connection ID.
    /// </summary>
    public string ConnectionId { get; set; } = string.Empty;
}
