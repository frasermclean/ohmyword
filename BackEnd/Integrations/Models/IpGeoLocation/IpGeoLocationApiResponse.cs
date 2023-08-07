using System.Text.Json.Serialization;

namespace OhMyWord.Infrastructure.Models.IpGeoLocation;

internal class IpGeoLocationApiResponse
{
    [JsonPropertyName("ip")] public string IpAddress { get; init; } = string.Empty;
    [JsonPropertyName("type")] public string IpVersion { get; init; } = string.Empty;
    [JsonPropertyName("city")] public City City { get; init; } = new();
    [JsonPropertyName("country")] public Country Country { get; init; } = new();
}
