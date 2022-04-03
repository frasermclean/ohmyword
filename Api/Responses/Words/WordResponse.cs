using OhMyWord.Data.Models;

namespace OhMyWord.Api.Responses.Words;

public class WordResponse
{
    private readonly Word word;

    public string Id => word.Id;
    public PartOfSpeech PartOfSpeech => word.PartOfSpeech;
    public string Definition => word.Definition;

    internal WordResponse(Word word)
    {
        this.word = word;
    }
}
