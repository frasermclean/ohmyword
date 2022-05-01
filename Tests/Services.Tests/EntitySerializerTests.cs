using FluentAssertions;
using OhMyWord.Core.Models;
using OhMyWord.Services.Data;
using System;
using System.IO;
using Xunit;

namespace Services.Tests;

public class EntitySerializerTests
{
    private const long OneTwoThree = 123;
    private readonly DateTime oneTwoThreeSecondsPastEpoch = new(1970, 1, 1, 0, 2, 3);

    [Fact]
    public void ConvertToStream_ShouldPass()
    {
        using var stream = EntitySerializer.ConvertToStream(TestWord);

        stream.Should().BeOfType<MemoryStream>();
        stream.Should().BeReadable();
    }

    [Fact]
    public void ConvertFromStream_ShouldPass()
    {
        var jsonString = File.ReadAllText("Data/rooster.json");
        using var stream = GenerateStreamFromString(jsonString);

        var word = EntitySerializer.ConvertFromStream<Word>(stream);

        word.Id.Should().Be("rooster");
        word.Definition.Should().Be("A male domestic chicken.");
        word.PartOfSpeech.Should().Be(PartOfSpeech.Noun);
        word.Timestamp.Should().Be(OneTwoThree);
        word.LastModifiedTime.Should().Be(oneTwoThreeSecondsPastEpoch);
    }

    [Fact]
    public void ConversionMethods_ShouldMatch()
    {
        using var stream = EntitySerializer.ConvertToStream(TestWord);
        var word = EntitySerializer.ConvertFromStream<Word>(stream);

        word.Id.Should().Be(TestWord.Id);
        word.Definition.Should().Be(TestWord.Definition);
        word.PartOfSpeech.Should().Be(TestWord.PartOfSpeech);
        word.Timestamp.Should().Be(OneTwoThree);
        word.LastModifiedTime.Should().Be(oneTwoThreeSecondsPastEpoch);
    }

    private static Word TestWord => new()
    {
        Id = "cat",
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