namespace OhMyWord.Api.Requests.Game;

public class GuessWordRequest
{
    public string PlayerId { get; set; } = default!;
    public string Value { get; init; } = default!;
}
