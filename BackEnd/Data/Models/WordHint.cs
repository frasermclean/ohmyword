namespace OhMyWord.Data.Models;

public class WordHint
{
    private readonly WordEntity entity;
    private readonly List<LetterHint> letterHints = new();

    public int Length => entity.Value.Length;
    public string Definition => entity.Definition;
    public IEnumerable<LetterHint> Letters => letterHints;

    internal WordHint(WordEntity entity)
    {
        this.entity = entity;
    }

    public static readonly WordHint Default = new(WordEntity.Default);

    public void AddLetterHint(LetterHint letterHint) => letterHints.Add(letterHint);
}
