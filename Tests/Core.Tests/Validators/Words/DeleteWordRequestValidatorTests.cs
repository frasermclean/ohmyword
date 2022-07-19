using System;
using FluentValidation.TestHelper;
using OhMyWord.Core.Requests.Words;
using OhMyWord.Core.Validators.Words;
using OhMyWord.Data.Models;
using Xunit;

namespace Core.Tests.Validators.Words;

public class DeleteWordRequestValidatorTests
{
    private readonly DeleteWordRequestValidator validator = new();

    [Fact]
    public void ValidRequest_Should_NotHaveAnyErrors()
    {
        // arrange
        var request = new DeleteWordRequest
        {
            Id = Guid.NewGuid(),            
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
        var request = new DeleteWordRequest();

        // act
        var result = validator.TestValidate(request);

        // assert        
        result.ShouldHaveValidationErrorFor(r => r.Id);        
        result.ShouldHaveValidationErrorFor(r => r.PartOfSpeech);
    }
}