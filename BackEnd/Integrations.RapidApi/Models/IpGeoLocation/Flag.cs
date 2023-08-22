using System.Text.Json.Serialization;

namespace OhMyWord.Integrations.RapidApi.Models.IpGeoLocation;

internal class Flag
{
    [JsonPropertyName("file")] public string? Url { get; set; }
}
