using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OhMyWord.Domain.Models;
using OhMyWord.Domain.Options;
using OhMyWord.Domain.Services;
using OhMyWord.Infrastructure.Models.Entities;

namespace Domain.Tests.Services;

public sealed class RoundFactoryTests
{
    private readonly IRoundFactory roundFactory;
    private readonly RoundOptions options = new() { LetterHintDelay = 3, PostRoundDelay = 5, GuessLimit = 3 };

    public RoundFactoryTests()
    {
        var playerServiceMock = new Mock<IPlayerService>();
        playerServiceMock.Setup(service => service.PlayerIds)
            .Returns(new[] { "abc123", "def456" });

        var wordsServiceMock = new Mock<IWordsService>();
        wordsServiceMock.Setup(service => service.GetRandomWordAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Word
            {
                Id = "random",
                Definitions = new[] { new Definition { PartOfSpeech = PartOfSpeech.Adjective, Value = "Random" } }
            });

        roundFactory = new RoundFactory(Options.Create(options), Mock.Of<ILogger<RoundFactory>>(),
            playerServiceMock.Object, wordsServiceMock.Object);
    }

    [Theory]
    [InlineData("test", 1, PartOfSpeech.Verb, "Test verb")]
    [InlineData("happy", 2, PartOfSpeech.Adjective, "Test adjective")]
    [InlineData("town", 3, PartOfSpeech.Noun, "Test noun")]
    public void CreateRound_Should_ReturnExpectedResults(string wordId, int roundNumber, PartOfSpeech partOfSpeech,
        string definition)
    {
        // arrange
        var word = new Word
        {
            Id = wordId, Definitions = new[] { new Definition { PartOfSpeech = partOfSpeech, Value = definition } }
        };

        // act
        using var round = roundFactory.CreateRound(word, roundNumber);

        // assert
        round.Id.Should().NotBeEmpty();
        round.Number.Should().Be(roundNumber);
        round.Word.Should().Be(word);
        round.WordHint.Length.Should().Be(wordId.Length);
        round.WordHint.Definition.Should().Be(definition);
        round.WordHint.PartOfSpeech.Should().Be(partOfSpeech);
        round.WordHint.LetterHints.Should().BeEmpty();
        round.GuessLimit.Should().Be(options.GuessLimit);
        round.StartDate.Should().BeBefore(DateTime.UtcNow);
        round.EndDate.Should().BeAfter(DateTime.UtcNow);
        round.PlayerCount.Should().Be(2);
    }

    [Fact]
    public async Task CreateRoundAsync_Should_ReturnExpectedResults()
    {
        // act
        using var round = await roundFactory.CreateRoundAsync(1);
        
        // assert
        round.Id.Should().NotBeEmpty();
        round.Number.Should().Be(1);
        round.Word.Id.Should().Be("random");
        round.WordHint.Length.Should().Be(6);
    }
}
