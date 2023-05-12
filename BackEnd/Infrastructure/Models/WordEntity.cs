namespace OhMyWord.Infrastructure.Models;

public sealed record WordEntity : Entity
{
    public int DefinitionCount { get; init; }
}
