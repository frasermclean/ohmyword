using FluentAssertions;
using OhMyWord.Core.Models;
using OhMyWord.Services.Data;
using System;
using System.IO;
using Xunit;

namespace Services.Tests;

public class EntitySerializerTests
{
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
        word.LastUpdateTime.Should().BeAfter(DateTime.UnixEpoch);
    }

    [Fact]
    public void ConversionMethods_ShouldMatch()
    {
        using var stream = EntitySerializer.ConvertToStream(TestWord);
        var word = EntitySerializer.ConvertFromStream<Word>(stream);

        word.Id.Should().Be(TestWord.Id);
        word.Definition.Should().Be(TestWord.Definition);
        word.PartOfSpeech.Should().Be(TestWord.PartOfSpeech);
    }

    private static Word TestWord => new()
    {
        Id = "cat",
        Definition = "Small furry creature",
        PartOfSpeech = PartOfSpeech.Noun,
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