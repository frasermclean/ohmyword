using OhMyWord.Core.Models;

namespace OhMyWord.Core.Tests.Models;

public class WordTests
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
        word.Bounty.Should().Be(0);
        word.LastModifiedTime.Should().Be(DateTime.MinValue);
        word.LastModifiedBy.Should().BeNull();
        word.ToString().Should().Be("default");
    }

    [Theory]
    [InlineData("dog", 5.34)]
    [InlineData("person", 5.4)]
    [InlineData("data", 4.32)]
    [InlineData("amoeba", 2.91)]
    [InlineData("firewall", 2.78)]
    public void Word_Should_Have_Expected_Properties(string id, double frequency)
    {
        // arrange
        var word = new Word { Id = id, Definitions = Enumerable.Empty<Definition>(), Frequency = frequency };
        var expectedBounty = Word.CalculateBounty(id.Length, frequency);

        // assert
        word.Id.Should().Be(id);
        word.Length.Should().Be(id.Length);
        word.Frequency.Should().Be(frequency);
        word.Bounty.Should().Be(expectedBounty);
    }
}
