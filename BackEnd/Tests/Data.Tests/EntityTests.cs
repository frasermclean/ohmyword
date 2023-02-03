using FluentAssertions;
using Xunit;

namespace Data.Tests;

public class EntityTests : IClassFixture<DataFixture>
{
    private readonly DataFixture fixture;

    public EntityTests(DataFixture fixture)
    {
        this.fixture = fixture;
    }

    [Fact]
    public void WordEntity_Should_HaveValidProperties()
    {
        // arrange
        var word = fixture.TestWord;

        // assert
        word.Id.Should().Be("test");
        word.DefinitionCount.Should().Be(1);
        word.Timestamp.Should().Be(123);
        word.LastModifiedTime.Should().Be(DateTime.UnixEpoch.AddSeconds(123));
        word.GetPartition().Should().Be("test");
    }
}
