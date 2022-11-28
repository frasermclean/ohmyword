using FluentAssertions;
using OhMyWord.Data;
using OhMyWord.Data.Models;
using Xunit;

namespace Data.Tests;

public class EntitySerializerTests
{
    private const long OneTwoThree = 123;
    private readonly DateTime oneTwoThreeSecondsPastEpoch = new(1970, 1, 1, 0, 2, 3);

    [Fact]
    public void ConvertToStream_ShouldPass()
    {
        var word = GetTestWord();
        using var stream = EntitySerializer.ConvertToStream(word);

        stream.Should().BeOfType<MemoryStream>();
        stream.Should().BeReadable();
    }

    [Fact]
    public void ConvertFromStream_ShouldPass()
    {
        var jsonString = File.ReadAllText("Data/rooster.json");
        using var stream = GenerateStreamFromString(jsonString);

        var word = EntitySerializer.ConvertFromStream<Word>(stream);

        word.Id.Should().Be("672d788e-ac53-416b-938d-f9903526c1a7");
        word.Value.Should().Be("rooster");
        word.Definition.Should().Be("A male domestic chicken.");
        word.PartOfSpeech.Should().Be(PartOfSpeech.Noun);
        word.Timestamp.Should().Be(OneTwoThree);
        word.LastModifiedTime.Should().Be(oneTwoThreeSecondsPastEpoch);
    }

    [Fact]
    public void ConversionMethods_ShouldMatch()
    {
        var testWord = GetTestWord();

        using var stream = EntitySerializer.ConvertToStream(testWord);
        var word = EntitySerializer.ConvertFromStream<Word>(stream);

        word.Id.Should().Be(testWord.Id);
        word.Definition.Should().Be(testWord.Definition);
        word.PartOfSpeech.Should().Be(testWord.PartOfSpeech);
        word.Timestamp.Should().Be(testWord.Timestamp);
        word.LastModifiedTime.Should().Be(testWord.LastModifiedTime);
    }

    private static Word GetTestWord() => new()
    {
        Value = "cat",
        Definition = "Small furry creature",
        PartOfSpeech = PartOfSpeech.Noun,
        Timestamp = OneTwoThree,
    };

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