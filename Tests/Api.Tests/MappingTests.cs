using AutoMapper;
using FluentAssertions;
using OhMyWord.Api.Mapping;
using OhMyWord.Api.Responses.Game;
using OhMyWord.Api.Responses.Words;
using OhMyWord.Core.Models;
using OhMyWord.Services.Events;
using OhMyWord.Services.Models;
using System;
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
        var response = mapper.Map<WordResponse>(TestWord);

        response.Value.Should().Be(TestWord.Value);
        response.PartOfSpeech.Should().Be(TestWord.PartOfSpeech);
        response.Definition.Should().Be(TestWord.Definition);
        response.LastModifiedTime.Should().Be(new DateTime(1970, 1, 1, 0, 2, 3));
    }

    [Fact]
    public void RoundStartResponseMappingShouldPass()
    {
        // arrange
        var args = new RoundStartedEventArgs
        {
            RoundId = TestRound.Id,
            RoundNumber = TestRound.Number,
            RoundEnds = DateTime.UtcNow + TimeSpan.FromSeconds(12)
        };

        // act
        var response = mapper.Map<RoundStartResponse>(args);

        // assert
        response.RoundId.Should().Be(args.RoundId);
        response.RoundNumber.Should().Be(args.RoundNumber);
        response.RoundEnds.Should().Be(args.RoundEnds);
        response.WordHint.Should().BeOfType<WordHint>();
    }

    [Fact]
    public void RoundEndResponseMappingShouldPass()
    {
        // arrange
        var args = new RoundEndedEventArgs
        {
            Round = new Round(1, TestWord, TimeSpan.FromSeconds(10)),
            NextRoundStart = DateTime.UtcNow + TimeSpan.FromSeconds(5)
        };

        // act
        var response = mapper.Map<RoundEndResponse>(args);

        // assert
        response.RoundNumber.Should().Be(args.Round.Number);
        response.RoundId.Should().Be(args.Round.Id);
        response.Word.Should().Be(args.Round.Word.Value);
        response.EndReason.Should().Be(args.Round.EndReason);
        response.NextRoundStart.Should().Be(args.NextRoundStart);
    }

    private static Word TestWord => new()
    {
        Id = "cat",
        Definition = "Small furry creature",
        PartOfSpeech = PartOfSpeech.Noun,
        Timestamp = 123
    };

    private static readonly Round TestRound = new(42, TestWord, TimeSpan.FromSeconds(7));
}