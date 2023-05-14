using OhMyWord.Infrastructure.Models.Entities;

namespace OhMyWord.Domain.Models;

public class WordHint
{
    private readonly List<LetterHint> letterHintHints = new();

    public int Length { get; }
    public string Definition { get; }
    public PartOfSpeech PartOfSpeech { get; }

    public IEnumerable<LetterHint> LetterHints => letterHintHints;

    public WordHint(Word word)
    {
        var definition = word.Definitions.ElementAt(Random.Shared.Next(word.Definitions.Count()));

        Length = word.Length;
        Definition = definition.Value;
        PartOfSpeech = definition.PartOfSpeech;
    }

    public static readonly WordHint Default = new(Word.Default);

    public void AddLetterHint(LetterHint letterHint) => letterHintHints.Add(letterHint);
}
