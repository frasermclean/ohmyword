using OhMyWord.Api.Events.RoundEnded;
using OhMyWord.Domain.Models;
using OhMyWord.Infrastructure.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Api.Tests.Events;

public class RoundEndedEventTests
{
    private static readonly Word TestWord = new()
    {
        Id = "test",
        Definitions = new List<Definition> { new() { PartOfSpeech = PartOfSpeech.Verb, Value = "Try" } },
    };

    private static readonly Round TestRound = new(1, TestWord, TimeSpan.FromSeconds(5), Enumerable.Empty<string>());

    [Fact]
    public void RoundEndedEvent_Should_Have_Expected_Values()
    {
        // arrange
        var nextRoundStart = DateTime.Now + TimeSpan.FromSeconds(10);
        var roundEndedEvent = new RoundEndedEvent(TestRound, nextRoundStart);

        // assert
        roundEndedEvent.Word.Should().Be("test");
        roundEndedEvent.PartOfSpeech.Should().Be(PartOfSpeech.Verb);
        roundEndedEvent.EndReason.Should().Be(RoundEndReason.Timeout);
        roundEndedEvent.NextRoundStart.Should().Be(nextRoundStart);
    }
}
