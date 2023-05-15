using OhMyWord.Domain.Models;
using OhMyWord.Infrastructure.Models.Entities;

namespace OhMyWord.Domain.Extensions;

public static class WordExtensions
{
    public static WordEntity ToEntity(this Word word) => new()
    {
        Id = word.Id,
        DefinitionCount = word.Definitions.Count()
    };
}
