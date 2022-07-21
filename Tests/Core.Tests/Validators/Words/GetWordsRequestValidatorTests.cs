using FluentAssertions;
using FluentValidation.TestHelper;
using OhMyWord.Core.Requests.Words;
using OhMyWord.Core.Validators.Words;
using OhMyWord.Data.Models;
using System;
using Xunit;

namespace Core.Tests.Validators.Words;

public class GetWordsRequestValidatorTests
{
    private readonly GetWordsRequestValidator validator = new();

    [Theory]
    [InlineData(0, 10, null, null, false)]
    [InlineData(5, 15, "dog", "value", true)]
    [InlineData(0, 10, "cat", "definition", false)]
    [InlineData(0, 3, "chicken", "partOfSpeech", true)]
    [InlineData(25, 20, "mouse", "lastModifiedTime", false)]
    public void ValidRequest_Should_NotHaveAnyErrors(int offset, int limit, string? filter, string? orderBy, bool desc)
    {
        // arrange
        var request = new GetWordsRequest
        {
            Offset = offset,
            Limit = limit,
            Filter = filter,
            OrderBy = orderBy,
            Desc = desc
        };

        // act
        var result = validator.TestValidate(request);

        // assert
        result.ShouldNotHaveAnyValidationErrors();
        request.Filter.Should().Be(filter);
        request.Desc.Should().Be(desc);
    }

    [Fact]
    public void InvalidRequest_Should_HaveErrors()
    {
        // arrange
        var request = new GetWordsRequest
        {
            Offset = -1,
            Limit = 0,            
            OrderBy = "something",            
        };
    
        // act
        var result = validator.TestValidate(request);
    
        // assert        
        result.ShouldHaveValidationErrorFor(r => r.Offset);
        result.ShouldHaveValidationErrorFor(r => r.Limit);
        result.ShouldHaveValidationErrorFor(r => r.OrderBy);
    }
}
