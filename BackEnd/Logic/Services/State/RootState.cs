namespace OhMyWord.Logic.Services.State;

public interface IRootState : IState
{
    IPlayerState PlayerState { get; }
    IRoundState RoundState { get; }
    ISessionState SessionState { get; }
}

public class RootState : IRootState
{
    public RootState(IPlayerState playerState, IRoundState roundState, ISessionState sessionState)
    {
        PlayerState = playerState;
        RoundState = roundState;
        SessionState = sessionState;
    }

    public IPlayerState PlayerState { get; }
    public IRoundState RoundState { get; }
    public ISessionState SessionState { get; }
    public bool IsDefault => PlayerState.IsDefault && SessionState.IsDefault && RoundState.IsDefault;

    public void Reset()
    {
        PlayerState.Reset();
        RoundState.Reset();
        SessionState.Reset();
    }
}
