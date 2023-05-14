using OhMyWord.Domain.Models;
using OhMyWord.Infrastructure.Models.Entities;
using OhMyWord.Infrastructure.Models.WordsApi;

namespace OhMyWord.Domain.Extensions;

public static class DefinitionResultExtensions
{
    public static Definition ToDefinition(this DefinitionResult result) => new()
    {
        Value = result.Definition,
        PartOfSpeech = Enum.TryParse<PartOfSpeech>(result.PartOfSpeech, true, out var partOfSpeech)
            ? partOfSpeech
            : PartOfSpeech.Unknown,
        Example = result.Examples.FirstOrDefault()
    };
}
