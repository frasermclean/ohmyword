using System.Text.Json.Serialization;

namespace OhMyWord.Infrastructure.Entities.Dictionary;

public class HeadwordInformation
{
    [JsonPropertyName("hw")] public string Headword { get; init; } = string.Empty;

    [JsonPropertyName("prs")]
    public IEnumerable<Pronunciation> Pronunciations { get; init; } = Enumerable.Empty<Pronunciation>();
}
