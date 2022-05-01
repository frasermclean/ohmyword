using OhMyWord.Core.Models;
using OhMyWord.Services.Options;

namespace OhMyWord.Services.Events;

public class RoundEndedEventArgs : EventArgs
{
    public Round Round { get; init; } = Round.Default;
    public DateTime NextRoundStart { get; init; } = DateTime.UtcNow + TimeSpan.FromSeconds(GameServiceOptions.PostRoundDelayDefault);
}
