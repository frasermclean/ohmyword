namespace OhMyWord.Api.Requests.Game;

public record SubmitGuessRequest(string PlayerId, string RoundId, string Value);
