using AutoFixture;
using OhMyWord.Core.Models;
using OhMyWord.Data.CosmosDb.Models;

namespace OhMyWord.Data.CosmosDb.Tests;

[Trait("Category", "Unit")]
public class CustomSerializerTests
{
    private readonly Fixture fixture = new();
    private readonly CustomSerializer serializer = CustomSerializer.Instance;

    [Fact]
    public void ConvertToStream_ShouldPass()
    {
        // arrange
        var word = fixture.Create<Word>();

        // act
        using var stream = serializer.ToStream(word);

        // assert
        stream.Should().BeOfType<MemoryStream>();
        stream.Should().BeReadable();
    }

    [Fact]
    public void ConvertFromStream_ShouldPass()
    {
        // arrange
        using var stream = GenerateStreamFromString(File.ReadAllText("rooster.json"));
        var expectedDefinition = new Definition
        {
            Id = Guid.Parse("5daa7ef8-0c5d-41b7-9976-428155248fb0"),
            PartOfSpeech = PartOfSpeech.Noun,
            Value = "a male chicken",
            Example = "The rooster crowed at dawn."
        };

        // act
        var wordItem = serializer.FromStream<WordItem>(stream);

        // assert
        wordItem.Id.Should().Be("rooster");
        wordItem.Definitions.Should().ContainSingle().Which.Should().BeEquivalentTo(expectedDefinition);
        wordItem.Frequency.Should().Be(3.14);
        wordItem.Timestamp.Should().Be(123);
        wordItem.LastModifiedTime.Should().Be(new DateTime(1970, 1, 1, 0, 2, 3));
    }

    [Fact]
    public void ConversionMethods_ShouldMatch()
    {
        // arrange
        var testWord = fixture.Create<WordItem>();

        // act
        using var stream = serializer.ToStream(testWord);
        var word = serializer.FromStream<WordItem>(stream);

        // assert
        word.Id.Should().Be(testWord.Id);
        word.Timestamp.Should().Be(testWord.Timestamp);
        word.Definitions.Should().BeEquivalentTo(testWord.Definitions);
        word.Frequency.Should().Be(testWord.Frequency);
        word.LastModifiedTime.Should().Be(testWord.LastModifiedTime);
        word.GetPartition().Should().Be(testWord.Id);
    }

    private static Stream GenerateStreamFromString(string input)
    {
        var stream = new MemoryStream();
        var streamWriter = new StreamWriter(stream);
        streamWriter.Write(input);
        streamWriter.Flush();
        stream.Position = 0;
        return stream;
    }
}
