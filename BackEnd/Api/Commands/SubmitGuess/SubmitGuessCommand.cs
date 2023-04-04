namespace OhMyWord.Api.Commands.SubmitGuess;

public class SubmitGuessCommand : ICommand<SubmitGuessResponse>
{
    public Guid RoundId { get; init; }
    public string Value { get; init; } = string.Empty;
    public string ConnectionId { get; init; } = string.Empty;
}
