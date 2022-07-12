using System.Text.Json.Serialization;

namespace OhMyWord.Core.Models;

public abstract class Entity
{
    public string Id { get; init; } = Guid.NewGuid().ToString();

    /// <summary>
    /// UNIX timestamp of last modification date.
    /// </summary>
    [JsonPropertyName("_ts")]
    public long Timestamp { get; init; }

    public DateTime LastModifiedTime => DateTimeOffset.FromUnixTimeSeconds(Timestamp).UtcDateTime;

    public abstract string GetPartition();
}
