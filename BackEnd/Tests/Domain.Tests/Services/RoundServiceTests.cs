using Microsoft.Extensions.Options;
using OhMyWord.Domain.Models;
using OhMyWord.Domain.Options;
using OhMyWord.Domain.Services;
using OhMyWord.Infrastructure.Models.Entities;

namespace Domain.Tests.Services;

public sealed class RoundServiceTests : IDisposable
{
    private readonly RoundService roundService;

    public RoundServiceTests()
    {
        var playerServiceMock = new Mock<IPlayerService>();
        playerServiceMock.Setup(service => service.PlayerIds)
            .Returns(new[] { "abc123", "def456" });

        var options = Options.Create(new RoundServiceOptions { LetterHintDelay = 3, PostRoundDelay = 5 });
        roundService = new RoundService(options, playerServiceMock.Object);
    }

    [Theory]
    [InlineData("test", 1, PartOfSpeech.Verb, "Test verb")]
    [InlineData("happy", 2, PartOfSpeech.Adjective, "Test adjective")]
    [InlineData("town", 3, PartOfSpeech.Noun, "Test noun")]
    public void StartRound_Should_ReturnExpectedResults(string wordId, int roundNumber, PartOfSpeech partOfSpeech,
        string definition)
    {
        // arrange
        var word = CreateTestWord(wordId, partOfSpeech, definition);

        // act
        var (data, _) = roundService.StartRound(word, roundNumber);

        // assert
        roundService.RoundNumber.Should().Be(roundNumber);
        roundService.IsRoundActive.Should().BeTrue();
        roundService.AllPlayersGuessed.Should().BeFalse();
        data.RoundNumber.Should().Be(roundNumber);
        data.RoundId.Should().NotBeEmpty();
        data.WordHint.Length.Should().Be(wordId.Length);
        data.WordHint.Definition.Should().Be(definition);
        data.WordHint.PartOfSpeech.Should().Be(partOfSpeech);
        data.WordHint.LetterHints.Should().BeEmpty();
        data.StartDate.Should().BeBefore(DateTime.UtcNow);
        data.EndDate.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public void EndRound_Should_ReturnExpectedResults()
    {
        // arrange
        var word = CreateTestWord("word", PartOfSpeech.Adverb, "Test adverb");

        // act
        roundService.StartRound(word, 1);
        var data = roundService.EndRound(RoundEndReason.AllPlayersGuessed);

        // assert
        roundService.LastEndReason.Should().Be(RoundEndReason.AllPlayersGuessed);
        data.RoundId.Should().NotBeEmpty();
        data.Word.Should().Be("word");
        data.EndReason.Should().Be(RoundEndReason.AllPlayersGuessed);
        data.PostRoundDelay.Should().Be(TimeSpan.FromSeconds(5));
        data.NextRoundStart.Should().BeAfter(DateTime.UtcNow);
    }

    private static Word CreateTestWord(string wordId, PartOfSpeech partOfSpeech, string definition) => new()
    {
        Id = wordId, Definitions = new[] { new Definition { PartOfSpeech = partOfSpeech, Value = definition } }
    };

    public void Dispose()
    {
        roundService.Dispose();
    }
}
