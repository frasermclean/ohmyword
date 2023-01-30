﻿using System.Text.Json.Serialization;

namespace OhMyWord.Data.Entities;

public abstract record Entity
{
    public string Id { get; init; } = Guid.NewGuid().ToString();

    /// <summary>
    /// UNIX timestamp of last modification date.
    /// </summary>
    [JsonPropertyName("_ts")]
    public long Timestamp { get; init; }

    public DateTime LastModifiedTime => DateTimeOffset.FromUnixTimeSeconds(Timestamp).UtcDateTime;

    public virtual string GetPartition() => Id;
}