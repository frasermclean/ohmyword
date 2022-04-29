using OhMyWord.Core.Models;

namespace OhMyWord.Services.Models.Events;

public class PlayerEventArgs : EventArgs
{
    private readonly Player player;
    public Player Player { get; }
    public int PlayerCount { get; }

    public PlayerEventArgs(Player player, int playerCount)
    {
        Player = player;
        PlayerCount = playerCount;
    }
}
