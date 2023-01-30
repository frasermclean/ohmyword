using OhMyWord.Core.Models;
using OhMyWord.Data.Entities;

namespace OhMyWord.Core.Extensions;

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
