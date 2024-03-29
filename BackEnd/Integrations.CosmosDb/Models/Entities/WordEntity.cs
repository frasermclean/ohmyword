﻿namespace OhMyWord.Integrations.CosmosDb.Models.Entities;

public sealed record WordEntity : Entity
{
    public int DefinitionCount { get; init; }
    public double Frequency { get; init; }
}
