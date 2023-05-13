using System.Text.Json.Serialization;

namespace OhMyWord.Infrastructure.Models.IpGeoLocation;

public class Flag
{
    [JsonPropertyName("file")] public string Url { get; set; } = string.Empty;
}
