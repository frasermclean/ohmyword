using OhMyWord.Data.Models;

namespace OhMyWord.Api.Responses.Words;

public class WordResponse
{
    public Guid Id { get; init; }
    public string Value { get; init; } = string.Empty;
    public PartOfSpeech PartOfSpeech { get; init; }
    public string Definition { get; init; } = string.Empty;
    public DateTime LastModifiedTime { get; init; }
}
