using System.Text.Json.Serialization;

namespace OhMyWord.Infrastructure.Models.IpGeoLocation;

internal class Flag
{
    [JsonPropertyName("file")] public string? Url { get; set; }
}
