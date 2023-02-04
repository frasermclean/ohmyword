namespace OhMyWord.Data.Entities;

public sealed record WordEntity : Entity
{
    public int DefinitionCount { get; init; }
}
