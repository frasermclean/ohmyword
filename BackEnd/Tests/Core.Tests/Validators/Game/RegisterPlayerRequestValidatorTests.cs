using FluentValidation.TestHelper;
using OhMyWord.Core.Requests.Game;
using OhMyWord.Core.Validators.Game;
using Xunit;

namespace Core.Tests.Validators.Game;

public class RegisterPlayerRequestValidatorTests
{
    private readonly RegisterPlayerRequestValidator validator = new();

    [Fact]
    public void ValidRequest_Should_NotHaveAnyErrors()
    {
        // arrange
        var request = new RegisterPlayerRequest
        {
            ConnectionId = "abc",
            VisitorId = "123"
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
        var request = new RegisterPlayerRequest();

        // act
        var result = validator.TestValidate(request);

        // assert        
        result.ShouldHaveValidationErrorFor(r => r.ConnectionId);
        result.ShouldHaveValidationErrorFor(r => r.VisitorId);
    }
}