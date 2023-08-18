namespace OhMyWord.Integrations.Models.WordsApi;

public class DefinitionResult
{
    public string Definition { get; init; } = string.Empty;
    public string PartOfSpeech { get; init; } = string.Empty;
    public IEnumerable<string> Examples { get; init; } = Enumerable.Empty<string>();
    public IEnumerable<string> Synonyms { get; init; } = Enumerable.Empty<string>();
    public IEnumerable<string> Antonyms { get; init; } = Enumerable.Empty<string>();
    public IEnumerable<string> Attribute { get; init; } = Enumerable.Empty<string>();
    public IEnumerable<string> TypeOf { get; init; } = Enumerable.Empty<string>();
}
