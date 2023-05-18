using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OhMyWord.Domain.Models;
using OhMyWord.Domain.Options;
using OhMyWord.Domain.Services;
using OhMyWord.Infrastructure.Models.Entities;

namespace Domain.Tests.Services;

public sealed class RoundFactoryTests
{
    private readonly RoundFactory roundFactory;

    public RoundFactoryTests()
    {
        var playerServiceMock = new Mock<IPlayerService>();
        playerServiceMock.Setup(service => service.PlayerIds)
            .Returns(new[] { "abc123", "def456" });

        var options = Options.Create(new RoundOptions { LetterHintDelay = 3, PostRoundDelay = 5, GuessLimit = 3 });
        roundFactory = new RoundFactory(options, Mock.Of<ILogger<RoundFactory>>(), playerServiceMock.Object);
    }

    [Theory]
    [InlineData("test", 1, PartOfSpeech.Verb, "Test verb")]
    [InlineData("happy", 2, PartOfSpeech.Adjective, "Test adjective")]
    [InlineData("town", 3, PartOfSpeech.Noun, "Test noun")]
    public void StartRound_Should_ReturnExpectedResults(string wordId, int roundNumber, PartOfSpeech partOfSpeech,
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
        round.GuessLimit.Should().Be(3);
        round.StartDate.Should().BeBefore(DateTime.UtcNow);
        round.EndDate.Should().BeAfter(DateTime.UtcNow);
    }
}
