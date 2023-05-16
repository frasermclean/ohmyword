using Microsoft.Extensions.Options;
using OhMyWord.Domain.Models;
using OhMyWord.Domain.Options;
using OhMyWord.Domain.Services;
using OhMyWord.Infrastructure.Models.Entities;

namespace Domain.Tests.Services;

public class RoundServiceTests
{
    private readonly IRoundService roundService;

    public RoundServiceTests()
    {
        var options = Options.Create(new RoundServiceOptions { LetterHintDelay = 3, PostRoundDelay = 5 });
        roundService = new RoundService(options);
    }

    [Theory]
    [InlineData("test", PartOfSpeech.Verb, "Test verb")]
    [InlineData("happy", PartOfSpeech.Adjective, "Test adjective")]
    public void StartRound_Should_ReturnExpectedResults(string wordId, PartOfSpeech partOfSpeech, string definition)
    {
        // arrange
        var word = CreateTestWord(wordId, partOfSpeech, definition);

        // act
        var (data, _) = roundService.StartRound(word);

        // assert
        roundService.RoundNumber.Should().Be(1);
        data.RoundNumber.Should().Be(1);
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
        roundService.StartRound(word);
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
}
