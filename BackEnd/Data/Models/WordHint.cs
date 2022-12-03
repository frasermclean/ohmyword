namespace OhMyWord.Data.Models;

public class WordHint
{
    private readonly Word word;
    private readonly List<LetterHint> letterHints = new();

    public int Length => word.Value.Length;
    public string Definition => word.Definition;
    public IEnumerable<LetterHint> Letters => letterHints;

    internal WordHint(Word word)
    {
        this.word = word;
    }

    public static readonly WordHint Default = new(Word.Default);

    public void AddLetterHint(LetterHint letterHint) => letterHints.Add(letterHint);
}
