namespace OhMyWord.Domain.Services.State;

public interface IState
{
    /// <summary>
    /// State is in default state.
    /// </summary>
    bool IsDefault { get; }
    
    /// <summary>
    /// Reset state to default.
    /// </summary>
    void Reset();
}
