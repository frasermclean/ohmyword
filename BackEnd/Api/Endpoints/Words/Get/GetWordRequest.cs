namespace OhMyWord.Api.Endpoints.Words.Get;

public class GetWordRequest
{
    public string WordId { get; init; } = string.Empty;

    public bool? PerformExternalLookup { get; init; }
}
