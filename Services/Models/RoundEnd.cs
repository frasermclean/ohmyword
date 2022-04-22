namespace OhMyWord.Services.Models;

public class RoundEnd
{
    public Guid RoundId { get; }
    public DateTime NextRoundStart { get; }

    internal RoundEnd(Round round, DateTime nextRoundStart)
    {
        RoundId = round.Id;
        NextRoundStart = nextRoundStart;
    }
}
