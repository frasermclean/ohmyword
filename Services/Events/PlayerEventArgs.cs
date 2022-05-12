namespace OhMyWord.Services.Events;

public class PlayerEventArgs : EventArgs
{
    public string PlayerId { get; }
    public int PlayerCount { get; }
    public string ConnectionId { get; }

    public PlayerEventArgs(string playerId, int playerCount, string connectionId)
    {
        PlayerId = playerId;
        PlayerCount = playerCount;
        ConnectionId = connectionId;
    }
}
