using System.Text.Json.Serialization;

namespace OhMyWord.Infrastructure.Entities.Dictionary;

public class DictionaryWord
{
    [JsonPropertyName("meta")] public EntryMetadata Metadata { get; init; } = new();
    [JsonPropertyName("hwi")] public HeadwordInformation HeadwordInformation { get; init; } = new();

    /// <summary>
    /// The functional label describes the grammatical function of a headword or undefined entry word, such as "noun" or "adjective".
    /// </summary>
    [JsonPropertyName("fl")]
    public string FunctionalLabel { get; init; } = string.Empty;

    [JsonPropertyName("shortdef")]
    public IEnumerable<string> ShortDefinitions { get; init; } = Enumerable.Empty<string>();
}
