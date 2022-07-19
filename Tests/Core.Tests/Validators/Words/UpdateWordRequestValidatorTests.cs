using System;
using FluentValidation.TestHelper;
using OhMyWord.Core.Requests.Words;
using OhMyWord.Core.Validators.Words;
using OhMyWord.Data.Models;
using Xunit;

namespace Core.Tests.Validators.Words;

public class UpdateWordRequestValidatorTests
{
    private readonly UpdateWordRequestValidator validator = new();

    [Fact]
    public void ValidRequest_Should_NotHaveAnyErrors()
    {
        // arrange
        var request = new UpdateWordRequest
        {
            Id = Guid.NewGuid(),
            Value = "dog",
            Definition = "Man's best friend",
            PartOfSpeech = PartOfSpeech.Noun
        };

        // act
        var result = validator.TestValidate(request);

        // assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void InvalidRequest_Should_HaveErrors()
    {
        // arrange
        var request = new UpdateWordRequest();

        // act
        var result = validator.TestValidate(request);

        // assert
        result.ShouldHaveValidationErrorFor(r => r.Id);
        result.ShouldHaveValidationErrorFor(r => r.Value);
        result.ShouldHaveValidationErrorFor(r => r.Definition);
        result.ShouldHaveValidationErrorFor(r => r.PartOfSpeech);
    }
}