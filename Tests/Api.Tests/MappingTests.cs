using AutoMapper;
using FluentAssertions;
using OhMyWord.Api.Mapping;
using OhMyWord.Api.Responses.Words;
using OhMyWord.Core.Models;
using Xunit;

namespace Api.Tests;

public class MappingTests
{
    private readonly MapperConfiguration mapperConfiguration;
    private readonly IMapper mapper;

    public MappingTests()
    {
        mapperConfiguration = new MapperConfiguration(config => config.AddProfiles(MappingProfiles.GetProfiles()));
        mapper = mapperConfiguration.CreateMapper();
    }

    [Fact]
    public void MappingConfigurationShouldBeValid()
    {
        mapperConfiguration.AssertConfigurationIsValid();
    }

    [Fact]
    public void WordResponseMappingShouldPass()
    {
        var word = new Word()
        {
            Id = "cat",
            Definition = "Small furry creature",
            PartOfSpeech = PartOfSpeech.Noun,
        };

        var (id, partOfSpeech, definition) = mapper.Map<WordResponse>(word);

        id.Should().Be(word.Id);
        partOfSpeech.Should().Be(word.PartOfSpeech);
        definition.Should().Be(word.Definition);
    }
}