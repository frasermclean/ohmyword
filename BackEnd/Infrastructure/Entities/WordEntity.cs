namespace OhMyWord.Infrastructure.Entities;

public sealed record WordEntity : Entity
{
    public int DefinitionCount { get; init; }
}
