namespace OhMyWord.Api.Responses.Game;

public class RegisterPlayerResponse
{
    public bool Successful { get; init; }
    public string PlayerId { get; init; } = string.Empty;
}
