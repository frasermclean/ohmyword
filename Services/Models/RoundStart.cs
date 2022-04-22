namespace OhMyWord.Services.Models;

public class RoundStart
{
    public Guid RoundId { get; }
    public int RoundNumber { get; }
    public DateTime RoundEnds { get; }

    internal RoundStart(Round round)
    {
        RoundId = round.Id;
        RoundNumber = round.Number;
        RoundEnds = round.Expiry;
    }
}
