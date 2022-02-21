namespace WhatTheWord.Domain.Responses.Game;

public class GetHintResponse
{
    public int Length { get; init; }
    public string Definition { get; init; } = default!;
    public DateTime Expiry { get; init; }
}
