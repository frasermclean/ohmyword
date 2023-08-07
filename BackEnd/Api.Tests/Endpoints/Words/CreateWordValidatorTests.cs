using OhMyWord.Api.Endpoints.Words.Create;
using OhMyWord.Api.Tests.Data;
using OhMyWord.Core.Models;

namespace OhMyWord.Api.Tests.Endpoints.Words;

[Trait("Category", "Unit")]
public class CreateWordValidatorTests
{
    private readonly CreateWordValidator validator = new();

    [Theory]
    [ClassData(typeof(WordsData))]
    public void Validate_WithValidRequest_ShouldPass(string wordId, PartOfSpeech partOfSpeech, string definition,
        double frequency)
    {
        // arrange
        var request = new CreateWordRequest
        {
            Id = wordId,
            Definitions = new[] { new Definition { PartOfSpeech = partOfSpeech, Value = definition } },
            Frequency = frequency
        };

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
        result.Errors.Should().HaveCount(4);
        result.Errors.Should().Contain(failure =>
            failure.PropertyName == "Id" && failure.ErrorMessage == "'Id' must not be empty.");
    }
}
