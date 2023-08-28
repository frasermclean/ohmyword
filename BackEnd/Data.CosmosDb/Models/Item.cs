using System.Text.Json.Serialization;

namespace OhMyWord.Data.CosmosDb.Models;

/// <summary>
/// Abstract base class for all items stored in Cosmos DB.
/// </summary>
public abstract record Item
{
    public string Id { get; init; } = Guid.NewGuid().ToString();

    /// <summary>
    /// UNIX timestamp of last modification date.
    /// </summary>
    [JsonPropertyName("_ts")]
    public long Timestamp { get; init; } = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

    public DateTime LastModifiedTime => DateTimeOffset.FromUnixTimeSeconds(Timestamp).UtcDateTime;

    public virtual string GetPartition() => Id;
}
