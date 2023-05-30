using Microsoft.Extensions.Logging;
using OhMyWord.Domain.Models;
using OhMyWord.Domain.Services;
using OhMyWord.Domain.Services.State;
using OhMyWord.Infrastructure.Models.Entities;

namespace Domain.Tests.Services;

[Trait("Category", "Unit")]
public class RootStateTests
{
    private readonly IRootState rootState;
    private readonly Mock<IRoundService> roundServiceMock = new();
    private readonly Mock<ILogger<RootState>> loggerMock = new();

    public RootStateTests()
    {
        var session = new Session();

        var word = new Word()
        {
            Id = "test",
            Definitions = new[] { new Definition { PartOfSpeech = PartOfSpeech.Verb, Value = "Test verb" } },
        };

        var round = new Round(word, TimeSpan.FromSeconds(3), new[] { Guid.NewGuid(), Guid.NewGuid() })
        {
            Number = 1, GuessLimit = 3, SessionId = session.Id
        };

        roundServiceMock.Setup(factory =>
                factory.CreateRoundAsync(It.IsAny<int>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(round);

        rootState = new RootState(loggerMock.Object, roundServiceMock.Object,
            new PlayerState(Mock.Of<ILogger<PlayerState>>()));
    }

    [Fact]
    public void DefaultObject_Should_Have_ExpectedValues()
    {
        // assert
        rootState.SessionState.Should().Be(SessionState.Waiting);
        rootState.Session.Should().BeNull();
        rootState.Round.Should().BeNull();
        rootState.IsDefault.Should().BeTrue();
        rootState.PlayerState.Should().NotBeNull();
    }

    [Fact]
    public void NextSession_Should_Set_ExpectedValues()
    {
        // act
        var session = rootState.NextSession();

        // assert
        session.Should().NotBeNull();
        rootState.Session.Should().Be(session);
        rootState.IsDefault.Should().BeFalse();
        rootState.SessionState.Should().Be(SessionState.RoundEnded);
    }

    [Fact]
    public async Task NextRoundAsync_Should_Set_ExpectedValues()
    {
        // act        
        var round = await rootState.NextRoundAsync();

        // assert
        round.Should().NotBeNull();
        rootState.Round.Should().Be(round);
        rootState.IsDefault.Should().BeFalse();
        rootState.SessionState.Should().Be(SessionState.RoundActive);
        rootState.IntervalStart.Should().Be(round.StartDate);
        rootState.IntervalEnd.Should().Be(round.EndDate);
    }

    [Fact]
    public async Task NextRoundAsync_WithEndingRound_Should_Set_ExpectedValues()
    {
        // act        
        var round = await rootState.NextRoundAsync();
        round.EndRound(RoundEndReason.AllPlayersGuessed);

        // assert
        round.Should().NotBeNull();
        rootState.Round.Should().Be(round);
        rootState.IsDefault.Should().BeFalse();
        rootState.SessionState.Should().Be(SessionState.RoundEnded);
        rootState.IntervalStart.Should().Be(round.StartDate);
        rootState.IntervalEnd.Should().Be(round.EndDate);
    }

    [Fact]
    public void Reset_Should_ResetStateToDefaults()
    {
        // act
        rootState.NextSession();
        rootState.Reset();

        // assert
        rootState.SessionState.Should().Be(SessionState.Waiting);
        rootState.Session.Should().BeNull();
        rootState.Round.Should().BeNull();
        rootState.IsDefault.Should().BeTrue();
    }

    [Fact]
    public async Task GetStateSnapshot_Should_ReturnExpectedValues()
    {
        // act
        await rootState.NextRoundAsync();
        var snapshot = rootState.GetStateSnapshot();

        // assert
        snapshot.RoundActive.Should().BeTrue();
        snapshot.RoundNumber.Should().Be(1);
        snapshot.RoundId.Should().NotBeEmpty();
        snapshot.Interval.StartDate.Should().BeBefore(DateTime.UtcNow);
        snapshot.Interval.EndDate.Should().BeAfter(DateTime.UtcNow);
        snapshot.WordHint.Should().NotBeNull();
    }
}
