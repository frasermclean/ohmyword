namespace OhMyWord.Services.Events;

public class PlayerEventArgs : EventArgs
{
    public string PlayerId { get; }
    public int PlayerCount { get; }

    public PlayerEventArgs(string playerId, int playerCount)
    {
        PlayerId = playerId;
        PlayerCount = playerCount;
    }
}
