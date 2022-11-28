using FluentAssertions;
using OhMyWord.Data.Models;
using Xunit;

namespace Data.Tests.Models;

public class ModelTests
{
    [Fact]
    public void WordShouldHaveValidProperties()
    {
        var timestamp = GetCurrentTimestamp();
        var word = GetTestWord(timestamp);

        word.Value.Should().Be("happy");
        word.Definition.Should().Be("A warm fuzzy feeling");
        word.PartOfSpeech.Should().Be(PartOfSpeech.Adjective);
        word.Timestamp.Should().Be(timestamp);
        word.LastModifiedTime.Should().Be(DateTime.UnixEpoch.AddSeconds(timestamp));
        word.GetPartition().Should().Be("adjective");
        word.ToString().Should().Be("happy");
    }

    [Fact]
    public void WordHintShouldHaveValidProperties()
    {
        var word = GetTestWord();
        var wordHint = word.GetWordHint();
        var letterHint = word.GetLetterHint(2);

        wordHint.Length.Should().Be(5);
        wordHint.Definition.Should().Be(word.Definition);
        wordHint.Letters.Should().HaveCount(0);
        letterHint.Value.Should().Be('a');

        // add a letter hint
        wordHint.AddLetterHint(letterHint);
        wordHint.Letters.Should().HaveCount(1);
    }

    [Theory]
    [InlineData(1, 'h')]
    [InlineData(3, 'p')]
    [InlineData(5, 'y')]
    public void LetterHintShouldHaveValidProperties(int position, char expectedValue)
    {
        var letterHint = GetTestWord().GetLetterHint(position);

        letterHint.Position.Should().Be(position);
        letterHint.Value.Should().Be(expectedValue);
    }

    [Fact]
    public void PlayerShouldHaveValidProperties()
    {
        var timestamp = GetCurrentTimestamp();
        var player = GetTestPlayer(timestamp);

        player.VisitorId.Should().Be("123");
        player.RegistrationCount.Should().Be(3);
        player.Score.Should().Be(400);
        player.Timestamp.Should().Be(timestamp);
        player.GetPartition().Should().Be(player.Id.ToString());
    }

    private static Player GetTestPlayer(long timestamp = default) => new()
    {
        VisitorId = "123",
        RegistrationCount = 3,
        Score = 400,
        Timestamp = timestamp,
    };

    private static Word GetTestWord(long timestamp = default) => new()
    {
        Value = "happy",
        Definition = "A warm fuzzy feeling",
        PartOfSpeech = PartOfSpeech.Adjective,
        Timestamp = timestamp
    };

    private static long GetCurrentTimestamp() => DateTimeOffset.Now.ToUnixTimeSeconds();
}