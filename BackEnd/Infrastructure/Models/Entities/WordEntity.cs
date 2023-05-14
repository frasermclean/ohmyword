namespace OhMyWord.Infrastructure.Models.Entities;

public sealed record WordEntity : Entity
{
    public int DefinitionCount { get; init; }
}
