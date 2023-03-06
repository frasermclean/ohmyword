using System.Text.Json.Serialization;

namespace OhMyWord.Infrastructure;

internal class CountResponse
{
    [JsonPropertyName("$1")]
    public int Count { get; init; }
}
