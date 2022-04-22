using OhMyWord.Core.Models;

namespace OhMyWord.Services.Models;

public class Round
{
    public Guid Id { get; } = Guid.NewGuid();
    public int Number { get; }
    public Word Word { get; }
    public WordHint WordHint { get; }
    public TimeSpan Duration { get; }
    public DateTime Expiry { get; }

    internal Round(int number, Word word, TimeSpan duration)
    {
        Number = number;
        Word = word;
        WordHint = new WordHint(word);
        Duration = duration;
        Expiry = DateTime.UtcNow + duration;
    }
}
