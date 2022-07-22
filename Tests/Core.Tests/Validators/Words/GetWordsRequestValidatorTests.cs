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
    [InlineData(0, 10, "", GetWordsOrderBy.Value, SortDirection.Ascending)]
    [InlineData(5, 15, "dog", GetWordsOrderBy.Value, SortDirection.Descending)]
    [InlineData(0, 10, "cat", GetWordsOrderBy.Definition, SortDirection.Ascending)]
    [InlineData(0, 3, "chicken", GetWordsOrderBy.PartOfSpeech, SortDirection.Descending)]
    [InlineData(25, 20, "mouse", GetWordsOrderBy.LastModifiedTime, SortDirection.Ascending)]
    public void ValidRequest_Should_NotHaveAnyErrors(int offset, int limit, string filter,
        GetWordsOrderBy orderBy, SortDirection direction)
    {
        // arrange
        var request = new GetWordsRequest
        {
            Offset = offset,
            Limit = limit,
            Filter = filter,
            OrderBy = orderBy,
            Direction = direction
        };

        // act
        var result = validator.TestValidate(request);

        // assert
        result.ShouldNotHaveAnyValidationErrors();
        request.Filter.Should().Be(filter);
        request.Direction.Should().Be(direction);
    }

    [Fact]
    public void InvalidRequest_Should_HaveErrors()
    {
        // arrange
        var request = new GetWordsRequest { Offset = -1, Limit = 0, Filter = null!, };

        // act
        var result = validator.TestValidate(request);

        // assert        
        result.ShouldHaveValidationErrorFor(r => r.Offset);
        result.ShouldHaveValidationErrorFor(r => r.Limit);
    }
}
