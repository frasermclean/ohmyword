using OhMyWord.Core.Services;
using OhMyWord.Integrations.RapidApi.Tests.Fixtures;

namespace OhMyWord.Integrations.RapidApi.Tests.Services;

[Trait("Category", "Integration")]
public class WordsApiClientTests : IClassFixture<RapidApiFixture>
{
    private readonly IDictionaryClient wordsApiClient;

    public WordsApiClientTests(RapidApiFixture fixture)
    {
        wordsApiClient = fixture.WordsApiClient;
    }

    [Theory]
    [InlineData("hello")]
    [InlineData("jump")]
    [InlineData("happy")]
    public async Task GetWord_WithValidWord_Should_ReturnExpectedResult(string wordId)
    {
        // act
        var word = await wordsApiClient.GetWordAsync(wordId);

        // assert
        word.Should().NotBeNull();
        word.Id.Should().Be(wordId);
        word.Length.Should().Be(wordId.Length);
        word.Frequency.Should().BePositive();
        word.Definitions.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetWordDetails_WithInvalidWord_Should_ReturnNull()
    {
        // act
        var word = await wordsApiClient.GetWordAsync("zyx");

        // assert
        word.Should().BeNull();
    }

    [Fact]
    public async Task GetRandomWord_Should_ReturnRandomDetails()
    {
        // act
        var word = await wordsApiClient.GetRandomWordAsync();

        // assert
        word.Should().NotBeNull();
    }
}
