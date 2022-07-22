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
    [InlineData(0, 10, "", GetWordsOrderBy.Value, false)]
    [InlineData(5, 15, "dog", GetWordsOrderBy.Value, true)]
    [InlineData(0, 10, "cat", GetWordsOrderBy.Definition, false)]
    [InlineData(0, 3, "chicken", GetWordsOrderBy.PartOfSpeech, true)]
    [InlineData(25, 20, "mouse", GetWordsOrderBy.LastModifiedTime, false)]
    public void ValidRequest_Should_NotHaveAnyErrors(int offset, int limit, string filter, GetWordsOrderBy orderBy, bool desc)
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
            Filter = null!
        };
    
        // act
        var result = validator.TestValidate(request);
    
        // assert        
        result.ShouldHaveValidationErrorFor(r => r.Offset);
        result.ShouldHaveValidationErrorFor(r => r.Limit);
        result.ShouldHaveValidationErrorFor(r => r.Filter);        
    }
}
