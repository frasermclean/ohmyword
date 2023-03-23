using OhMyWord.Infrastructure.Enums;

namespace Infrastructure.Tests;

[Trait("Category", "Unit")]
public class EntityTests : IClassFixture<DataFixture>
{
    private readonly DataFixture fixture;

    public EntityTests(DataFixture fixture)
    {
        this.fixture = fixture;
    }

    [Fact]
    public void WordEntity_Should_HaveValidProperties()
    {
        // arrange
        var word = fixture.TestWord;

        // assert
        word.Id.Should().Be("test");
        word.DefinitionCount.Should().Be(1);
        word.Timestamp.Should().Be(123);
        word.LastModifiedTime.Should().Be(DateTime.UnixEpoch.AddSeconds(123));
        word.GetPartition().Should().Be("test");
    }

    [Fact]
    public void DefinitionEntity_Should_HaveValidProperties()
    {
        var definition = fixture.TestDefinition;
        var partition = definition.GetPartition();

        // assert
        definition.Id.Should().Be("10271ba9-60ec-4073-8552-14dbb477a895");
        definition.Value.Should().Be("Test definition");
        definition.PartOfSpeech.Should().Be(PartOfSpeech.Noun);
        definition.Example.Should().Be("Test example");
        definition.WordId.Should().Be("test");
        partition.Should().Be("test");
    }

    [Fact]
    public void PlayerEntity_Should_HaveValidProperties()
    {
        var player = fixture.TestPlayer;
        var partition = player.GetPartition();

        // assert
        player.Id.Should().Be("abc123");
        player.RegistrationCount.Should().Be(3);
        player.Score.Should().Be(400);
        partition.Should().Be("abc123");
    }
}
