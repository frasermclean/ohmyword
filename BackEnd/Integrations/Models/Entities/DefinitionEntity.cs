using OhMyWord.Core.Models;

namespace OhMyWord.Integrations.Models.Entities;

public record DefinitionEntity : Entity
{
    public string Value { get; init; } = string.Empty;
    public PartOfSpeech PartOfSpeech { get; init; }
    public string? Example { get; init; }
    public string WordId { get; init; } = string.Empty;

    public override string GetPartition() => WordId;
}
