using AutoMapper;
using OhMyWord.Core.Models;
using OhMyWord.Data.CosmosDb.Mapping;
using OhMyWord.Data.CosmosDb.Models;

namespace OhMyWord.Data.CosmosDb.Tests.Mapping;

public class WordsProfileTests
{
    private readonly MapperConfiguration configuration;
    private readonly IMapper mapper;

    public WordsProfileTests()
    {
        configuration = new MapperConfiguration(config => config.AddProfile<WordsProfile>());
        mapper = configuration.CreateMapper();
    }

    [Fact(DisplayName = "Mapper configuration should be valid")]
    public void MapperConfiguration_Should_BeValid()
    {
        configuration.AssertConfigurationIsValid();
    }

    [Theory(DisplayName = "Mapping from Word to WordItem should pass"), AutoData]
    public void MappingFromSessionToSessionItem_Should_Pass(Word word)
    {
        // act
        var wordItem = mapper.Map<WordItem>(word);

        // assert
        wordItem.Should().NotBeNull();
        wordItem.Id.Should().Be(word.Id);
        wordItem.Definitions.Should().BeEquivalentTo(word.Definitions);
        wordItem.Frequency.Should().Be(word.Frequency);
        wordItem.LastModifiedBy.Should().Be(word.LastModifiedBy);
        wordItem.LastModifiedTime.Should().BeCloseTo(word.LastModifiedTime, TimeSpan.FromHours(12));
        wordItem.GetPartition().Should().Be(word.Id);
    }

    [Theory(DisplayName = "Mapping from WordItem to Word should pass"), AutoData]
    public void MappingFromWordItemToWord_Should_Pass(WordItem wordItem)
    {
        // act
        var word = mapper.Map<Word>(wordItem);

        // assert
        word.Should().NotBeNull();
        word.Id.Should().Be(wordItem.Id);
        word.Length.Should().Be(wordItem.Id.Length);
        word.Definitions.Should().BeEquivalentTo(wordItem.Definitions);
        word.Frequency.Should().Be(wordItem.Frequency);
        word.LastModifiedBy.Should().Be(wordItem.LastModifiedBy);
        word.LastModifiedTime.Should().BeCloseTo(wordItem.LastModifiedTime, TimeSpan.FromHours(12));
    }
}
