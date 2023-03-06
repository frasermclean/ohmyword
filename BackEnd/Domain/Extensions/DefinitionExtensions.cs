using OhMyWord.Domain.Models;
using OhMyWord.Infrastructure.Entities;

namespace OhMyWord.Domain.Extensions;

public static class DefinitionExtensions
{
    public static DefinitionEntity ToEntity(this Definition definition, string wordId) => new()
    {
        Id = definition.Id.ToString(),
        PartOfSpeech = definition.PartOfSpeech,
        Value = definition.Value,
        Example = definition.Example,
        WordId = wordId,
    };
}
