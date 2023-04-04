using OhMyWord.Domain.Models;
using OhMyWord.Infrastructure.Entities.Dictionary;
using OhMyWord.Infrastructure.Enums;

namespace OhMyWord.Domain.Extensions;

public static class DictionaryWordExtensions
{
    public static Word ToWord(this DictionaryWord dictionaryWord) => new()
    {
        Id = dictionaryWord.Metadata.Stems.First(),
        Definitions = dictionaryWord.ShortDefinitions.Select(definition => new Definition
        {
            PartOfSpeech = ParseFunctionalLabel(dictionaryWord.FunctionalLabel), Value = definition,
        })
    };

    private static PartOfSpeech ParseFunctionalLabel(string functionalLabel) =>
        Enum.TryParse<PartOfSpeech>(functionalLabel, true, out var partOfSpeech) ? partOfSpeech : PartOfSpeech.Unknown;
}
