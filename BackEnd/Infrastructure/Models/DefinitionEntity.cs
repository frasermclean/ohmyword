namespace OhMyWord.Infrastructure.Models;

public record DefinitionEntity : Entity
{
    public string Value { get; init; } = string.Empty;
    public PartOfSpeech PartOfSpeech { get; init; }
    public string? Example { get; init; }
    public string WordId { get; init; } = string.Empty;

    public override string GetPartition() => WordId;
}
