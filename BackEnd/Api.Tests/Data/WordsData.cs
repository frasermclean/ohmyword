using OhMyWord.Core.Models;
using System.Collections;

namespace OhMyWord.Api.Tests.Data;

public class WordsData : IEnumerable<object[]>
{
    private static readonly List<object[]> Data = new()
    {
        new object[] { "house", PartOfSpeech.Noun, "Building for human habitation", 5.1d },
        new object[] { "jump", PartOfSpeech.Verb, "To propel oneself upward", 4.8d },
        new object[] { "happy", PartOfSpeech.Adjective, "Feeling or showing joy or pleasure", 3.8d },
        new object[] { "quickly", PartOfSpeech.Adverb, "At a fast pace", 4.5d }
    };

    public IEnumerator<object[]> GetEnumerator() => Data.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => Data.GetEnumerator();
}
