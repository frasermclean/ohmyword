using OhMyWord.Domain.Models;
using OhMyWord.Infrastructure.Models.Entities;

namespace OhMyWord.Api.Events.RoundEnded;

public class RoundEndedEvent : IEvent
{
    private readonly Round round;

    public string Word => round.Word.Id;
    public PartOfSpeech PartOfSpeech => round.WordHint.PartOfSpeech;
    public RoundEndReason EndReason => round.EndReason;
    public DateTime NextRoundStart { get; }
    public IEnumerable<ScoreLine> Scores { get; }

    public RoundEndedEvent(Round round, DateTime nextRoundStart)
    {
        this.round = round;
        NextRoundStart = nextRoundStart;
    }
}
