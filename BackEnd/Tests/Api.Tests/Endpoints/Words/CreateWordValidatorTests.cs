﻿using OhMyWord.Api.Endpoints.Words.Create;
using OhMyWord.Api.Tests.Data;
using OhMyWord.Domain.Models;
using OhMyWord.Infrastructure.Models.Entities;

namespace OhMyWord.Api.Tests.Endpoints.Words;

[Trait("Category", "Unit")]
public class CreateWordValidatorTests
{
    private readonly CreateWordValidator validator = new();

    [Theory]
    [ClassData(typeof(WordsData))]
    public void Validate_WithValidRequest_ShouldPass(string wordId, PartOfSpeech partOfSpeech, string definition)
    {
        // arrange
        var request = CreateRequest(wordId, partOfSpeech, definition);

        // act
        var result = validator.Validate(request);

        // assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithInvalidRequest_ShouldFail()
    {
        // arrange
        var request = new CreateWordRequest();

        // act
        var result = validator.Validate(request);

        // assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(2);
        result.Errors.Should().Contain(failure =>
            failure.PropertyName == "Id" && failure.ErrorMessage == "'Id' must not be empty.");
    }

    private static CreateWordRequest CreateRequest(string wordId, PartOfSpeech partOfSpeech, string definition) =>
        new()
        {
            Id = wordId, Definitions = new[] { new Definition { PartOfSpeech = partOfSpeech, Value = definition } }
        };
}
