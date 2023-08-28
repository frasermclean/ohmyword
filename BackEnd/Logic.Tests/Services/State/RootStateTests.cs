using Microsoft.Extensions.Logging;
using OhMyWord.Domain.Services;
using OhMyWord.Domain.Services.State;

namespace OhMyWord.Domain.Tests.Services.State;

[Trait("Category", "Unit")]
public class RootStateTests
{
    private readonly IRootState rootState;

    public RootStateTests()
    {
        var playerState = new PlayerState(Mock.Of<ILogger<PlayerState>>());
        var sessionState = new SessionState(Mock.Of<ILogger<SessionState>>());
        var roundState = new RoundState(Mock.Of<ILogger<RoundState>>(), sessionState, Mock.Of<IRoundService>());

        rootState = new RootState(playerState, roundState, sessionState);
    }

    [Fact]
    public void DefaultObject_Should_Have_ExpectedValues()
    {
        // assert        
        rootState.IsDefault.Should().BeTrue();
        rootState.PlayerState.IsDefault.Should().BeTrue();
        rootState.RoundState.IsDefault.Should().BeTrue();
        rootState.SessionState.IsDefault.Should().BeTrue();
    }

    [Fact]
    public void Reset_Should_ResetStateToDefaults()
    {
        // act
        rootState.SessionState.NextSession();
        rootState.Reset();

        // assert
        rootState.IsDefault.Should().BeTrue();
    }
}
