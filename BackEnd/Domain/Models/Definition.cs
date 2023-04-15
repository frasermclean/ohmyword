using OhMyWord.Infrastructure.Entities;
using OhMyWord.Infrastructure.Enums;
using System.Text.Json.Serialization;

namespace OhMyWord.Domain.Models;

public class Definition
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required PartOfSpeech PartOfSpeech { get; init; }
    public required string Value { get; init; }

    /// <summary>
    /// Example of this <see cref="Definition"/> used in a sentence.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Example { get; init; }

    public static Definition FromEntity(DefinitionEntity entity) => new()
    {
        Id = Guid.TryParse(entity.Id, out var id) ? id : Guid.Empty,
        PartOfSpeech = entity.PartOfSpeech,
        Value = entity.Value,
        Example = entity.Example,
    };
}
