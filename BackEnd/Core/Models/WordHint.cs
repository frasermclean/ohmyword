using System.Text.Json.Serialization;

namespace OhMyWord.Core.Models;

public class WordHint
{
    public required int Length { get; init; }
    public required string Definition { get; init; }
    [JsonIgnore] public required Guid DefinitionId { get; init; }
    public required PartOfSpeech PartOfSpeech { get; init; }
    public List<LetterHint> LetterHints { get; } = new();

    public static readonly WordHint Default = new()
    {
        Length = Word.Default.Length,
        Definition = "Default word",
        DefinitionId = Guid.Empty,
        PartOfSpeech = PartOfSpeech.Noun,
    };
}
