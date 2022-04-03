﻿namespace OhMyWord.Data.Models;

public class Word : Entity
{
    public string Definition { get; init; } = string.Empty;
    public PartOfSpeech PartOfSpeech { get; init; }

    public override string ToString() => Id;

    public static readonly Word Default = new();
}
