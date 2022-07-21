﻿using FluentAssertions;
using FluentValidation.TestHelper;
using OhMyWord.Core.Requests.Words;
using OhMyWord.Core.Validators.Words;
using OhMyWord.Data.Models;
using Xunit;

namespace Core.Tests.Validators.Words;

public class CreateWordRequestValidatorTests
{
    private readonly CreateWordRequestValidator validator = new();

    [Fact]
    public void ValidRequest_Should_NotHaveAnyErrors()
    {
        // arrange
        var request = new CreateWordRequest
        {
            Value = "dog",
            Definition = "Man's best friend",
            PartOfSpeech = PartOfSpeech.Noun
        };

        // act
        var result = validator.TestValidate(request);
        var word = request.ToWord();

        // assert
        result.ShouldNotHaveAnyValidationErrors();
        word.Value.Should().Be(request.Value);
        word.Definition.Should().Be(request.Definition);
        word.PartOfSpeech.Should().Be(request.PartOfSpeech);
    }

    [Fact]
    public void InvalidRequest_Should_HaveErrors()
    {
        // arrange
        var request = new CreateWordRequest();

        // act
        var result = validator.TestValidate(request);

        // assert        
        result.ShouldHaveValidationErrorFor(r => r.Value);
        result.ShouldHaveValidationErrorFor(r => r.Definition);
        result.ShouldHaveValidationErrorFor(r => r.PartOfSpeech);
    }
}
