namespace OhMyWord.Core.Models;

public class Word
{
    public const int MinLength = 4;
    public const int MaxLength = 16;

    public required string Id { get; init; } = string.Empty;

    /// <summary>
    /// Number of characters in the word.
    /// </summary>
    public int Length => Id.Length;

    public required IEnumerable<Definition> Definitions { get; init; }

    /// <summary>
    /// Logarithmic frequency of the word ranging from 1 to 7. Higher is more frequent.
    /// </summary>
    public required double Frequency { get; init; }

    /// <summary>
    /// User ID of the user who last modified the word.
    /// </summary>
    public required Guid? LastModifiedBy { get; init; }

    public DateTime LastModifiedTime { get; init; } = DateTime.UtcNow;

    public override string ToString() => Id;

    public static readonly Word Default = new()
    {
        Id = "default",
        Definitions = new List<Definition> { new() { PartOfSpeech = PartOfSpeech.Noun, Value = "Default word" } },
        Frequency = default,
        LastModifiedBy = Guid.Empty,
        LastModifiedTime = DateTime.MinValue,
    };
}
