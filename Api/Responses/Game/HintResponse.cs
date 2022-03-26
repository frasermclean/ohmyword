using WhatTheWord.Data.Models;

namespace WhatTheWord.Api.Responses.Game;

public class HintResponse
{
    private readonly Word word;

    public int Length => word.Value.Length;
    public string Definition => word.Definition;
    public DateTime Expiry { get; }

    public HintResponse(Word word, DateTime expiry)
    {
        this.word = word;
        Expiry = expiry;
    }
}
