namespace OhMyWord.Core.Models;

public class WordHint
{
    private readonly Word word;
    private readonly List<LetterHint> letterHints = new();

    public int Length => word.Id.Length;
    public string Definition => word.Definition;
    public IReadOnlyList<LetterHint> Letters => letterHints;

    public WordHint(Word word)
    {
        this.word = word;
    }

    public static readonly WordHint Default = new(Word.Default);

    public void AddLetterHint(LetterHint letterHint) => letterHints.Add(letterHint);
}
