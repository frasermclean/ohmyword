using OhMyWord.Core.Models;
using OhMyWord.Services.Options;

namespace OhMyWord.Services.Events;

public class RoundEndedEventArgs : EventArgs
{
    public Round Round { get; }
    public DateTime NextRoundStart { get; }

    public RoundEndedEventArgs(Round round, DateTime nextRoundStart)
    {
        Round = round;
        NextRoundStart = nextRoundStart;
    }
}
