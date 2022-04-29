using OhMyWord.Core.Models;

namespace OhMyWord.Services.Models;

public class RoundStart
{
    private readonly Round round;

    public Guid RoundId => round.Id;
    public int RoundNumber => round.Number;
    public DateTime RoundEnds => round.Expiry;
    public WordHint WordHint => round.WordHint;

    internal RoundStart(Round round)
    {
        this.round = round;
    }
}
