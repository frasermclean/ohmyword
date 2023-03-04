using System.Text.Json.Serialization;

namespace OhMyWord.Functions.Models;

public class GetUserClaimsResponse
{
    [JsonPropertyName("role")] public string Role { get; init; } = string.Empty;
}
