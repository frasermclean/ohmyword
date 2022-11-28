using OhMyWord.Data.Models;

namespace OhMyWord.Core.Events;

public class RoundStartedEventArgs : EventArgs
{
    public Round Round { get; }

    public RoundStartedEventArgs(Round round)
    {
        Round = round;
    }
}
