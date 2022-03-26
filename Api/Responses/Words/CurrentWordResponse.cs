using WhatTheWord.Data.Models;

namespace WhatTheWord.Api.Responses.Words;

public class CurrentWordResponse
{
    public Word Word { get; init; } = default!;
    public DateTime Expiry { get; init; }
}
