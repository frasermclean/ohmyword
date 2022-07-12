using FluentAssertions;
using OhMyWord.Core.Models;
using Xunit;

namespace Core.Tests;

public class ModelTests
{
    [Fact]
    public void WordShouldHaveValidProperties()
    {
        var timestamp = DateTimeOffset.Now.ToUnixTimeSeconds();
        var word = GetTestWord(timestamp);

        word.Id.Should().Be("happy");
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
        var wordHint = new WordHint(word);
        var letterHint = new LetterHint
        {
            Position = 2,
            Value = 'a'
        };

        wordHint.Length.Should().Be(5);
        wordHint.Definition.Should().Be(word.Definition);
        wordHint.Letters.Should().HaveCount(0);

        // add a letter hint
        wordHint.AddLetterHint(letterHint);
        wordHint.Letters.Should().HaveCount(1);
    }

    private static Word GetTestWord(long timestamp = 0)
    {
        return new Word
        {
            Id = "happy",
            Definition = "A warm fuzzy feeling",
            PartOfSpeech = PartOfSpeech.Adjective,
            Timestamp = timestamp
        };
    }
}