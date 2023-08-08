using System.Text.Json.Serialization;

namespace OhMyWord.Core.Models;

public class WordHint
{
    public int Length { get; }
    public string Definition { get; }
    [JsonIgnore] public Guid DefinitionId { get; }
    public PartOfSpeech PartOfSpeech { get; }

    public List<LetterHint> LetterHints { get; } = new();

    public WordHint(Word word)
    {
        var definition = word.Definitions.ElementAt(Random.Shared.Next(word.Definitions.Count()));

        Length = word.Length;
        Definition = definition.Value;
        DefinitionId = definition.Id;
        PartOfSpeech = definition.PartOfSpeech;
    }

    public static readonly WordHint Default = new(Word.Default);
}
