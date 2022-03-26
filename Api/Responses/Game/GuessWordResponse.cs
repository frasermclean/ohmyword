namespace OhMyWord.Api.Responses.Game;

public class GuessWordResponse
{
    public string Value { get; init; } = default!;
    public bool Correct { get; init; }
}
