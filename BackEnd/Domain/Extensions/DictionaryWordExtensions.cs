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
            PartOfSpeech = ParseFunctionalLabel(dictionaryWord.FunctionalLabel),
            Value = definition,            
        })
    };

    private static PartOfSpeech ParseFunctionalLabel(string functionalLabel) => functionalLabel switch
    {
        "noun" => PartOfSpeech.Noun,
        "verb" => PartOfSpeech.Verb,
        "adjective" => PartOfSpeech.Adjective,
        "adverb" => PartOfSpeech.Adverb,
        _ => throw new InvalidDataException($"Unhandled functional label: {functionalLabel}")
    };
}
