using OhMyWord.Data.Enums;

namespace OhMyWord.Core.Models;

public class Word
{
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// Number of characters in the word.
    /// </summary>
    public int Length => Id.Length;

    public IEnumerable<Definition> Definitions { get; init; } = Enumerable.Empty<Definition>();
    public required DateTime LastModified { get; init; }

    public LetterHint GetLetterHint(int position) => new(position, Id[position - 1]);

    public static readonly Word Default = new()
    {
        Id = "default",
        Definitions = new List<Definition> { new() { PartOfSpeech = PartOfSpeech.Noun, Value = "Default word" } },
        LastModified = DateTime.UtcNow,
    };
}
