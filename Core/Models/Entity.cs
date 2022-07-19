using System.Text.Json.Serialization;

namespace OhMyWord.Core.Models;

public abstract class Entity
{
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>
    /// UNIX timestamp of last modification date.
    /// </summary>
    [JsonPropertyName("_ts")]
    public long Timestamp { get; init; }

    public DateTime LastModifiedTime => DateTimeOffset.FromUnixTimeSeconds(Timestamp).UtcDateTime;

    public virtual string GetPartition() => Id.ToString();
}
