using System.Text.Json.Serialization;

namespace OhMyWord.Infrastructure.Models.IpGeoLocation;

public class IpGeoLocationData
{
    [JsonPropertyName("ip")] public string IpAddress { get; set; } = string.Empty;
    [JsonPropertyName("type")] public IpVersion IpVersion { get; set; }
}
