using WhatTheWord.Data.Models;

namespace WhatTheWord.Domain.Responses.Game;

public class GetHintResponse
{
    private readonly Word word;

    public int Length => word.Value.Length;
    public string Definition => word.Definition;
    public DateTime Expiry { get; }

    internal GetHintResponse(Word word, DateTime expiry)
    {
        this.word = word;
        Expiry = expiry;
    }
}
