namespace OhMyWord.Core.Models;

public abstract record Entity
{
    /// <summary>
    /// Unique identifier for the <see cref="Entity"/>
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// The time this <see cref="Entity"/> was last modified.
    /// </summary>
    public DateTime LastModifiedTime { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Invalid ID to be used for default values.
    /// </summary>
    protected static string InvalidId => Guid.Empty.ToString();
}
