using OhMyWord.Domain.Models;

namespace OhMyWord.Domain.Services;

public interface IStateProvider
{
    Session Session { get; }
    Round Round { get; set; }
    bool IsDefault { get; }

    Session StartSession();
    int IncrementRoundCount();
    void Reset();
}

public class StateProvider : IStateProvider
{
    public Session Session { get; private set; } = Session.Default;
    public Round Round { get; set; } = Round.Default;

    public bool IsDefault => Session == Session.Default && Round == Round.Default;

    public Session StartSession() => Session = new Session();
    public int IncrementRoundCount() => Session.RoundCount++;

    public void Reset()
    {
        Session = Session.Default;
        Round = Round.Default;
    }
}
