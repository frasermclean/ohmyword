namespace OhMyWord.Core.Models;

public class WordHint
{
    public required int Length { get; init; }
    public required Definition Definition { get; init; }
    public List<LetterHint> LetterHints { get; } = new();

    public static readonly WordHint Default = new() { Length = Word.Default.Length, Definition = Definition.Default };

    public static WordHint FromWord(Word word) => new()
    {
        Length = word.Length, Definition = word.Definitions.ElementAt(Random.Shared.Next(word.Definitions.Count()))
    };
}
