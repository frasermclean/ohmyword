using OhMyWord.Core.Models;

namespace OhMyWord.Api.Responses.Game;

public class Hint
{
    private readonly Word word;

    public int Length => word.Id.Length;
    public string Definition => word.Definition;
    public DateTime Expiry { get; }

    public Hint(Word word, DateTime expiry)
    {
        this.word = word;
        Expiry = expiry;
    }

    public static readonly Hint Default = new(Word.Default, DateTime.MinValue);
}
