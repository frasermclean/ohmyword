using OhMyWord.Infrastructure.Models.Entities;
using OhMyWord.Infrastructure.Tests.Models;

namespace OhMyWord.Infrastructure.Tests;

[Trait("Category", "Unit")]
public class EntitySerializerTests : IClassFixture<DataFixture>
{
    private readonly DataFixture fixture;
    private readonly EntitySerializer serializer = EntitySerializer.Instance;

    public EntitySerializerTests(DataFixture fixture)
    {
        this.fixture = fixture;
    }

    [Fact]
    public void ConvertToStream_ShouldPass()
    {
        var word = fixture.TestWord;
        using var stream = serializer.ToStream(word);

        stream.Should().BeOfType<MemoryStream>();
        stream.Should().BeReadable();
    }

    [Fact]
    public void ConvertFromStream_ShouldPass()
    {
        var jsonString = File.ReadAllText("rooster.json");
        using var stream = GenerateStreamFromString(jsonString);

        var word = serializer.FromStream<WordEntity>(stream);

        word.Id.Should().Be("rooster");
        word.Timestamp.Should().Be(123);
        word.LastModifiedTime.Should().Be(new DateTime(1970, 1, 1, 0, 2, 3));
    }

    [Fact]
    public void ConversionMethods_ShouldMatch()
    {
        var testWord = fixture.TestWord;

        using var stream = EntitySerializer.ConvertToStream(testWord);
        var word = EntitySerializer.ConvertFromStream<WordEntity>(stream);

        word.Id.Should().Be(testWord.Id);
        word.Timestamp.Should().Be(testWord.Timestamp);
        word.LastModifiedTime.Should().Be(testWord.LastModifiedTime);
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
