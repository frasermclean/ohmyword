using OhMyWord.Core.Models;

namespace OhMyWord.Services.Models;

public class RoundEnd
{
    private readonly Round round;

    public Guid RoundId => round.Id;
    public RoundEndReason EndReason => round.EndReason;
    public DateTime NextRoundStart { get; }
    public string Word => round.Word.Value;

    internal RoundEnd(Round round, DateTime nextRoundStart)
    {
        this.round = round;
        NextRoundStart = nextRoundStart;
    }
}
