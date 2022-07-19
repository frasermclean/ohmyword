namespace OhMyWord.Services.Events;

public class PlayerEventArgs : EventArgs
{
    public Guid PlayerId { get; }
    public int PlayerCount { get; }
    public string ConnectionId { get; }

    public PlayerEventArgs(Guid playerId, int playerCount, string connectionId)
    {
        PlayerId = playerId;
        PlayerCount = playerCount;
        ConnectionId = connectionId;
    }
}
