using Microsoft.Extensions.Logging;
using OhMyWord.Domain.Models;
using OhMyWord.Domain.Services.State;

namespace OhMyWord.Domain.Tests.Services.State;

public class PlayerStateTests
{
    private readonly IPlayerState playerState;

    public PlayerStateTests()
    {
        playerState = new PlayerState(Mock.Of<ILogger<PlayerState>>());
    }

    [Fact]
    public void DefaultState_Should_Have_ExpectedValues()
    {
        // assert
        playerState.PlayerCount.Should().Be(0);
        playerState.PlayerIds.Should().BeEmpty();
    }

    [Theory, AutoData]
    public void AddPlayer_Should_Return_ExpectedResults(Player player)
    {
        // act
        var result1 = playerState.AddPlayer(player);
        var result2 = playerState.AddPlayer(player);

        // assert
        result1.Should().BeTrue();
        result2.Should().BeFalse();
        playerState.PlayerCount.Should().Be(1);
        playerState.PlayerIds.Should().Contain(player.Id);
        playerState.PlayerIds.Should().HaveCount(1);
    }

    [Theory, AutoData]
    public void RemovePlayer_Should_Return_ExpectedResults(Player player)
    {
        // act
        var result1 = playerState.RemovePlayer(player.ConnectionId);
        playerState.AddPlayer(player);
        var result2 = playerState.RemovePlayer(player.ConnectionId);

        // assert
        result1.Should().BeFalse();
        result2.Should().BeTrue();
        playerState.PlayerCount.Should().Be(0);
        playerState.PlayerIds.Should().NotContain(player.Id);
        playerState.PlayerIds.Should().BeEmpty();
    }

    [Theory, AutoData]
    public void GetPlayerById_Should_Return_ExpectedResults(Player player)
    {
        // act
        playerState.AddPlayer(player);
        var result = playerState.GetPlayerById(player.Id);

        // assert
        result.Should().Be(player);
    }

    [Theory, AutoData]
    public void GetPlayerByConnectionId_Should_Return_ExpectedResults(Player player)
    {
        // act
        playerState.AddPlayer(player);
        var result = playerState.GetPlayerByConnectionId(player.ConnectionId);

        // assert
        result.Should().Be(player);
    }


    [Theory, AutoData]
    public void Reset_Should_Return_ExpectedResults(Player player)
    {
        // act
        playerState.AddPlayer(player);
        playerState.Reset();

        // assert
        playerState.PlayerCount.Should().Be(0);
        playerState.PlayerIds.Should().BeEmpty();
    }
}
