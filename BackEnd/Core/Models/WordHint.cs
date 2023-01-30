namespace OhMyWord.Core.Models;

public class WordHint
{
    private readonly List<LetterHint> letterHints = new();

    public int Length { get; }
    public string Definition { get; }
    public IEnumerable<LetterHint> Letters => letterHints;

    internal WordHint(Word word)
    {
        var definitionIndex = Random.Shared.Next(word.Definitions.Count());

        Length = word.Length;
        Definition = word.Definitions.ElementAt(definitionIndex).Value;
    }

    public static readonly WordHint Default = new(Word.Default);

    public void AddLetterHint(LetterHint letterHint) => letterHints.Add(letterHint);
}
