namespace WhatTheWord.Data.Models;

public class Word : Entity
{
    public string Value { get; set; } = default!;
    public string Definition { get; set; } = default!;
}
