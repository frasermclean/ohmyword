using System.Text.Json.Serialization;

namespace OhMyWord.Infrastructure.Entities.Dictionary;

public class EntryMetadata
{
    public string Id { get; init; } = string.Empty;
    public Guid Uuid { get; init; }
    public string Sort { get; init; } = string.Empty;
    [JsonPropertyName("src")] public string Source { get; init; } = string.Empty;
    public string Section { get; init; } = string.Empty;
    public IEnumerable<string> Stems { get; init; } = Enumerable.Empty<string>();    
    [JsonPropertyName("offensive")] public bool IsOffensive { get; init; }
}
