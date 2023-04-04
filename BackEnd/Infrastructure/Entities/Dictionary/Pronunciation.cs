using System.Text.Json.Serialization;

namespace OhMyWord.Infrastructure.Entities.Dictionary;

public class Pronunciation
{
    [JsonPropertyName("mw")] public string WrittenForm { get; init; } = string.Empty;
    [JsonPropertyName("sound")] public Sound Sound { get; init; } = new();
}
