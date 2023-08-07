using OhMyWord.Infrastructure.Services.RapidApi.WordsApi;
using OhMyWord.Integrations.Tests.Fixtures;

namespace OhMyWord.Integrations.Tests.Services.RapidApi.WordsApi;

[Trait("Category", "Integration")]
public class WordsApiClientTests : IClassFixture<RapidApiFixture>
{
    private readonly IWordsApiClient wordsApiClient;

    public WordsApiClientTests(RapidApiFixture fixture)
    {
        wordsApiClient = fixture.WordsApiClient;
    }

    [Theory]
    [InlineData("hello", "noun")]
    [InlineData("jump", "verb")]
    [InlineData("happy", "adjective")]
    public async Task GetWordDetails_WithValidWord_Should_ReturnExpectedResult(string word, string expectedPartOfSpeech)
    {
        // act
        var details = await wordsApiClient.GetWordDetailsAsync(word);
        var firstResult = details?.DefinitionResults.FirstOrDefault();

        // assert
        Assert.NotNull(details);
        details.Word.Should().Be(word);
        details.Frequency.Should().BeGreaterThan(0);
        Assert.NotNull(firstResult);
        firstResult.Definition.Should().NotBeEmpty();
        firstResult.PartOfSpeech.Should().Be(expectedPartOfSpeech);
    }

    [Fact]
    public async Task GetWordDetails_WithInvalidWord_Should_ReturnNull()
    {
        // act
        var details = await wordsApiClient.GetWordDetailsAsync("zyx");

        // assert
        Assert.Null(details);
    }

    [Fact]
    public async Task GetRandomWord_Should_ReturnRandomDetails()
    {
        // act
        var details = await wordsApiClient.GetRandomWordDetailsAsync();

        // assert
        Assert.NotNull(details);
    }
}
