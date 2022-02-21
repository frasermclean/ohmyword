using WhatTheWord.Data.Models;

namespace WhatTheWord.Domain.Processing.Words;

public class GetAllWordsResponse
{
    public IEnumerable<Word> Words { get; }

    public GetAllWordsResponse(IEnumerable<Word> words)
    {
        Words = words;
    }
}
