﻿using OhMyWord.Infrastructure.Entities;
using OhMyWord.Infrastructure.Enums;

namespace Infrastructure.Tests;

public class DataFixture
{
    public WordEntity TestWord { get; } = new() { Id = "test", DefinitionCount = 1, Timestamp = 123 };

    public DefinitionEntity TestDefinition { get; } = new()
    {
        Id = "10271ba9-60ec-4073-8552-14dbb477a895",
        Value = "Test definition",
        PartOfSpeech = PartOfSpeech.Noun,
        Example = "Test example",
        WordId = "test"
    };

    public VisitorEntity TestVisitor { get; } = new() { Id = "abc123", RegistrationCount = 3, Score = 400 };
}