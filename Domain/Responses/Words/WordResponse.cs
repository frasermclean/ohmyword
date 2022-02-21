using WhatTheWord.Data.Models;

namespace WhatTheWord.Domain.Responses.Words;

public class WordResponse
{
    private readonly Word word;

    public Guid Id => word.Id;
    public string Value => word.Value;
    public string Definition => word.Definition;

    internal WordResponse(Word word)
    {
        this.word = word;
    }
}
