namespace WhatTheWord.Domain.Processing.Words;

public class GetCurrentWordResponse
{
    public int Length { get; init; }
    public string Definition { get; init; } = default!;
    public DateTime Expiry { get; init; }
}
