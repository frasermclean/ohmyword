using OhMyWord.Core.Models;

namespace OhMyWord.Services.Events;

public class PlayerEventArgs : EventArgs
{
    public Player Player { get; }
    public int PlayerCount { get; }

    public PlayerEventArgs(Player player, int playerCount)
    {
        Player = player;
        PlayerCount = playerCount;
    }
}
