using Microsoft.Extensions.Logging;
using OhMyWord.Domain.Models;
using OhMyWord.Domain.Services;
using OhMyWord.Infrastructure.Models.Entities;

namespace Domain.Tests.Services;

[Trait("Category", "Unit")]
public class StateManagerTests
{
    private readonly IStateManager stateManager;
    private readonly Mock<IRoundService> roundServiceMock = new();
    private readonly Mock<ILogger<StateManager>> loggerMock = new();

    public StateManagerTests()
    {
        var session = new Session();

        var word = new Word()
        {
            Id = "test",
            Definitions = new[] { new Definition { PartOfSpeech = PartOfSpeech.Verb, Value = "Test verb" } },
        };

        var round = new Round(word, 3, new[] { "abc", "def" }) { Number = 1, GuessLimit = 3, SessionId = session.Id };

        roundServiceMock.Setup(factory =>
                factory.CreateRoundAsync(It.IsAny<int>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(round);

        stateManager = new StateManager(loggerMock.Object, roundServiceMock.Object);
    }

    [Fact]
    public void DefaultObject_Should_Have_ExpectedValues()
    {
        // assert
        stateManager.SessionState.Should().Be(SessionState.Waiting);
        stateManager.Session.Should().BeNull();
        stateManager.Round.Should().BeNull();
        stateManager.IsDefault.Should().BeTrue();
    }

    [Fact]
    public void NextSession_Should_Set_ExpectedValues()
    {
        // act
        var session = stateManager.NextSession();

        // assert
        session.Should().NotBeNull();
        stateManager.Session.Should().Be(session);
        stateManager.IsDefault.Should().BeFalse();
        stateManager.SessionState.Should().Be(SessionState.RoundEnded);
    }

    [Fact]
    public async Task NextRoundAsync_Should_Set_ExpectedValues()
    {
        // act        
        var round = await stateManager.NextRoundAsync();

        // assert
        round.Should().NotBeNull();
        stateManager.Round.Should().Be(round);
        stateManager.IsDefault.Should().BeFalse();
        stateManager.SessionState.Should().Be(SessionState.RoundActive);
        stateManager.IntervalStart.Should().Be(round.StartDate);
        stateManager.IntervalEnd.Should().Be(round.EndDate);
    }

    [Fact]
    public async Task NextRoundAsync_WithEndingRound_Should_Set_ExpectedValues()
    {
        // act        
        var round = await stateManager.NextRoundAsync();
        round.EndRound(RoundEndReason.AllPlayersGuessed);

        // assert
        round.Should().NotBeNull();
        stateManager.Round.Should().Be(round);
        stateManager.IsDefault.Should().BeFalse();
        stateManager.SessionState.Should().Be(SessionState.RoundEnded);
        stateManager.IntervalStart.Should().Be(round.StartDate);
        stateManager.IntervalEnd.Should().Be(round.EndDate);
    }

    [Fact]
    public void Reset_Should_ResetStateToDefaults()
    {
        // act
        stateManager.NextSession();
        stateManager.Reset();

        // assert
        stateManager.SessionState.Should().Be(SessionState.Waiting);
        stateManager.Session.Should().BeNull();
        stateManager.Round.Should().BeNull();
        stateManager.IsDefault.Should().BeTrue();
    }
}
