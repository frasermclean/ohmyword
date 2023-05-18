using OhMyWord.Infrastructure.Models.Entities;

namespace OhMyWord.Domain.Models;

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
    public DateTime LastModifiedTime { get; init; }

    public LetterHint GetLetterHint(int position) => new(position, Id[position - 1]);

    public override string ToString() => Id;

    public static readonly Word Default = new()
    {
        Id = "default",
        Definitions = new List<Definition> { new() { PartOfSpeech = PartOfSpeech.Noun, Value = "Default word" } },
        LastModifiedTime = DateTime.UtcNow,
    };
}
