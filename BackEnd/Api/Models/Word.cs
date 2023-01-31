using OhMyWord.Data.Enums;

namespace OhMyWord.Api.Models;

public class Word
{
    public required string Id { get; init; } = string.Empty;

    /// <summary>
    /// Number of characters in the word.
    /// </summary>
    public int Length => Id.Length;

    public required IEnumerable<Definition> Definitions { get; init; }
    public DateTime LastModifiedTime { get; init; }

    public LetterHint GetLetterHint(int position) => new(position, Id[position - 1]);

    public static readonly Word Default = new()
    {
        Id = "default",
        Definitions = new List<Definition> { new() { PartOfSpeech = PartOfSpeech.Noun, Value = "Default word" } },
        LastModifiedTime = DateTime.UtcNow,
    };
}
