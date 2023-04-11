using System.Text.Json.Serialization;

namespace OhMyWord.WordsApi.Models;

public class WordDetails
{
    public string Word { get; init; } = string.Empty;

    [JsonPropertyName("results")]
    public IEnumerable<DefinitionResult> DefinitionResults { get; init; } = Enumerable.Empty<DefinitionResult>();
}
