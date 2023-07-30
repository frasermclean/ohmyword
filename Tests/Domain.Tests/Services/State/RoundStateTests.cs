using Microsoft.Extensions.Logging;
using OhMyWord.Domain.Models;
using OhMyWord.Domain.Services;
using OhMyWord.Domain.Services.State;
using OhMyWord.Infrastructure.Models.Entities;

namespace OhMyWord.Domain.Tests.Services.State;

public class RoundStateTests
{
    private readonly IRoundState roundState;
    private readonly Mock<IRoundService> roundServiceMock = new();

    public RoundStateTests()
    {
        roundServiceMock.Setup(factory =>
                factory.CreateRoundAsync(It.IsAny<int>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((int roundNumber, Guid sessionId, CancellationToken _) =>
                new Round(Word.Default, TimeSpan.FromSeconds(5), new[] { Guid.NewGuid(), Guid.NewGuid() })
                {
                    Number = roundNumber, GuessLimit = 3, SessionId = sessionId
                });

        var sessionState = new SessionState(Mock.Of<ILogger<SessionState>>());
        roundState = new RoundState(Mock.Of<ILogger<RoundState>>(), sessionState, roundServiceMock.Object);
    }

    [Fact]
    public async Task NextRoundAsync_Should_Set_ExpectedValues()
    {
        // act        
        var round = await roundState.NextRoundAsync();

        // assert
        round.Should().NotBeNull();
        roundState.RoundId.Should().Be(round.Id);
        roundState.RoundNumber.Should().Be(round.Number);
        roundState.IsDefault.Should().BeFalse();
        roundState.Interval.StartDate.Should().Be(round.StartDate);
        roundState.Interval.EndDate.Should().Be(round.EndDate);
    }

    [Fact]
    public async Task NextRoundAsync_WithEndingRound_Should_Set_ExpectedValues()
    {
        // act        
        var round = await roundState.NextRoundAsync();
        round.EndRound(RoundEndReason.AllPlayersGuessed);

        // assert
        round.Should().NotBeNull();
        roundState.IsDefault.Should().BeFalse();
        roundState.IsActive.Should().BeFalse();
        roundState.Interval.StartDate.Should().Be(round.StartDate);
        roundState.Interval.EndDate.Should().Be(round.EndDate);
    }

    [Theory, AutoData]
    public async Task ProcessGuess_Should_ReturnExpectedValues(Guid playerId)
    {
        // act        
        var round = await roundState.NextRoundAsync();
        round.AddPlayer(playerId);

        // act
        var result = roundState.ProcessGuess(playerId, round.Id, Word.Default.Id);

        // assert
        result.Should().BeSuccess();
        result.Value.Should().BePositive();
    }

    [Fact]
    public async Task GetSnapshot_Should_ReturnExpectedValues()
    {
        // act
        await roundState.NextRoundAsync();
        var snapshot = roundState.GetSnapshot();

        // assert
        snapshot.RoundActive.Should().BeTrue();
        snapshot.RoundNumber.Should().Be(1);
        snapshot.RoundId.Should().NotBeEmpty();
        snapshot.Interval.StartDate.Should().BeBefore(DateTime.UtcNow);
        snapshot.Interval.EndDate.Should().BeAfter(DateTime.UtcNow);
        snapshot.WordHint.Should().NotBeNull();
    }
}
