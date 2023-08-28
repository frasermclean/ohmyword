using System.Text.Json.Serialization;

namespace OhMyWord.Data.CosmosDb;

internal class CountResponse
{
    [JsonPropertyName("$1")] public int Count { get; init; }
}
