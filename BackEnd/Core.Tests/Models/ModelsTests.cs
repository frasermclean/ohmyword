using OhMyWord.Core.Models;

namespace OhMyWord.Core.Tests.Models;

public class ModelsTests
{
    [Fact]
    public void DefaultWord_Should_Have_Expected_Properties()
    {
        // arrange
        var word = Word.Default;

        // assert
        word.Id.Should().Be("default");
        word.Length.Should().Be(7);
        word.Definitions.Single().Should().Be(Definition.Default);
        word.Frequency.Should().Be(0);
        word.LastModifiedTime.Should().Be(DateTime.MinValue);
        word.LastModifiedBy.Should().BeEmpty();
        word.ToString().Should().Be("default");
    }

    [Fact]
    public void DefaultWordHint_Should_Have_Expected_Properties()
    {
        // arrange
        var wordHint = WordHint.Default;

        // assert
        wordHint.Length.Should().Be(7);
        wordHint.Definition.Should().Be(Definition.Default);
        wordHint.LetterHints.Should().BeEmpty();
    }

    [Fact]
    public void WordHint_Should_Have_ExpectedProperties()
    {
        // arrange
        var definition = new Definition { PartOfSpeech = PartOfSpeech.Noun, Value = "test" };
        var wordHint = new WordHint { Length = 5, Definition = definition, };
        var now = DateTime.Now;

        // act
        wordHint.LetterHints.Add(new LetterHint(3, 's') { CreatedAt = now });
        wordHint.LetterHints.Add(new LetterHint(1, 't') { CreatedAt = now });
        wordHint.LetterHints.Add(new LetterHint(4, 'e') { CreatedAt = now });

        // assert
        wordHint.Length.Should().Be(5);
        wordHint.Definition.Should().Be(definition);
        wordHint.LetterHints.Should().HaveCount(3);
        wordHint.LetterHints.Should().AllSatisfy(letterHint =>
        {
            letterHint.Position.Should().BeOneOf(1, 3, 4);
            letterHint.Value.Should().BeOneOf('t', 's', 'e');
            letterHint.CreatedAt.Should().Be(now);
        });
    }

    [Fact]
    public void WordHint_FromWord_Should_Have_Expected_Properties()
    {
        // arrange
        var definition = new Definition { PartOfSpeech = PartOfSpeech.Noun, Value = "test" };
        var word = new Word
        {
            Id = "test",
            Definitions = new[] { definition },
            Frequency = 4,
            LastModifiedBy = Guid.Empty,
            LastModifiedTime = DateTime.Now
        };

        // act
        var wordHint = WordHint.FromWord(word);

        // assert
        wordHint.Length.Should().Be(4);
        wordHint.Definition.Should().Be(definition);
        wordHint.LetterHints.Should().BeEmpty();
    }
}
