using OhMyWord.Infrastructure.Models.Entities;
using System.Collections;

namespace OhMyWord.Api.Tests.Data;

public class WordsData : IEnumerable<object[]>
{
    private static readonly List<object[]> Data = new()
    {
        new object[] { "house", PartOfSpeech.Noun, "Building for human habitation" },
        new object[] { "jump", PartOfSpeech.Verb, "To propel oneself upward" },
        new object[] { "happy", PartOfSpeech.Adjective, "Feeling or showing joy or pleasure" },
        new object[] { "quickly", PartOfSpeech.Adverb, "At a fast pace" }
    };

    public IEnumerator<object[]> GetEnumerator() => Data.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => Data.GetEnumerator();
}
