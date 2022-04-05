namespace OhMyWord.Core.Models;

public class WordHint
{
    private readonly Word word;

    public int Length => word.Id.Length;
    public string Definition => word.Definition;
    public List<LetterHint> Letters { get; } = new();

    public WordHint(Word word)
    {
        this.word = word;
    }
}
