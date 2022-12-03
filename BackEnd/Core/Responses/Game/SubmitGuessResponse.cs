namespace OhMyWord.Core.Responses.Game;

public class SubmitGuessResponse
{
    public string Value { get; init; } = default!;
    public bool Correct { get; init; }
    public int Points { get; init; }
}
