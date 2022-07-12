using OhMyWord.Core.Models;

namespace OhMyWord.Services.Events;

public class RoundStartedEventArgs : EventArgs
{
    public Round Round { get; }

    public RoundStartedEventArgs(Round round)
    {
        Round = round;
    }
}
