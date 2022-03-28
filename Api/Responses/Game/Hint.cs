using OhMyWord.Data.Models;

namespace OhMyWord.Api.Responses.Game;

public class Hint
{
    private readonly Word word;

    public int Length => word.Value.Length;
    public string Definition => word.Definition;
    public DateTime Expiry { get; }

    public Hint(Word word, DateTime expiry)
    {
        this.word = word;
        Expiry = expiry;
    }

    public static readonly Hint Default = new (Word.Default, DateTime.MinValue);
}
