using OhMyWord.Data.Entities;
using OhMyWord.Data.Enums;

namespace OhMyWord.Core.Models;

public class Definition
{
    public PartOfSpeech PartOfSpeech { get; init; }
    public string Value { get; init; } = string.Empty;
    
    /// <summary>
    /// Example of this <see cref="Definition"/> used in a sentence.
    /// </summary>
    public string Example { get; init; } = string.Empty;

    public static Definition FromEntity(DefinitionEntity entity) => new()
    {
        PartOfSpeech = entity.PartOfSpeech, Value = entity.Value, Example = entity.Example,
    };
}
