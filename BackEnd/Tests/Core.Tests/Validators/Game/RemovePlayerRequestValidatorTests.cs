using FluentValidation.TestHelper;
using OhMyWord.Core.Requests.Game;
using OhMyWord.Core.Validators.Game;
using Xunit;

namespace Core.Tests.Validators.Game;

public class RemovePlayerRequestValidatorTests
{
    private readonly RemovePlayerRequestValidator validator = new();

    [Fact]
    public void ValidRequest_Should_NotHaveAnyErrors()
    {
        // arrange
        var request = new RemovePlayerRequest
        {
            ConnectionId = "abc",
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
        var request = new RemovePlayerRequest();

        // act
        var result = validator.TestValidate(request);

        // assert        
        result.ShouldHaveValidationErrorFor(r => r.ConnectionId);
    }
}