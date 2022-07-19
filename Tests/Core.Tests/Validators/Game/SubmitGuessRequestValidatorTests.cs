using System;
using FluentValidation.TestHelper;
using OhMyWord.Core.Requests.Game;
using OhMyWord.Core.Validators.Game;
using Xunit;

namespace Core.Tests.Validators.Game;

public class SubmitGuessRequestValidatorTests
{
    private readonly SubmitGuessRequestValidator validator = new();

    [Fact]
    public void ValidRequest_Should_NotHaveAnyErrors()
    {
        // arrange
        var request = new SubmitGuessRequest
        {
            RoundId = Guid.NewGuid(),
            Value = "hello",
            ConnectionId = "abc"
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
        var request = new SubmitGuessRequest();

        // act
        var result = validator.TestValidate(request);

        // assert        
        result.ShouldHaveValidationErrorFor(r => r.RoundId);
        result.ShouldHaveValidationErrorFor(r => r.Value);
        result.ShouldHaveValidationErrorFor(r => r.ConnectionId);
    }
}