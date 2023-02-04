using OhMyWord.Data.Entities;

namespace OhMyWord.Seeder.Models;

public class WordWithDefinitions
{
    public string Id { get; init; } = string.Empty;
    public List<DefinitionEntity> Definitions { get; init; } = new();
}
