using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using OhMyWord.Core.Models;
using OhMyWord.Services.Data.Repositories;
using OhMyWord.Services.Game;
using Xunit;

namespace Services.Tests;

public class PlayerServiceTests
{
    private readonly Mock<IPlayerRepository> playerRepositoryMock;
    private readonly IPlayerService playerService;

    public PlayerServiceTests()
    {
        var loggerMock = new Mock<ILogger<PlayerService>>();
        playerRepositoryMock = new Mock<IPlayerRepository>();
        playerService = new PlayerService(loggerMock.Object, playerRepositoryMock.Object);
    }

    [Fact]
    public void DefaultProperties_Should_BeAsExpected()
    {
        // assert
        playerService.PlayerCount.Should().Be(0);
        playerService.PlayerIds.Should().BeEmpty();
    }

    [Fact]
    public async Task AddPlayer_WithExistingPlayer_Should_IncreaseRegistrationCount()
    {
        // arrange      
        const string visitorId = "123";
        const string connectionId = "abc";
        playerRepositoryMock.Setup(repo => repo.FindPlayerByVisitorIdAsync(visitorId))
            .ReturnsAsync(new Player { VisitorId = visitorId });
        var monitor = playerService.Monitor();

        // act
        var player = await playerService.AddPlayerAsync(visitorId, connectionId);

        // assert
        player.VisitorId.Should().Be(visitorId);
        player.RegistrationCount.Should().BeGreaterThan(0);
        playerService.PlayerIds.Should().Contain(player.Id);
        playerService.PlayerCount.Should().BeGreaterThan(0);
        monitor.Should().Raise(nameof(IPlayerService.PlayerAdded));
    }

    [Fact]
    public async Task AddPlayer_WithNewPlayer_Should_IncreaseRegistrationCount()
    {
        // arrange      
        const string visitorId = "123";
        const string connectionId = "abc";
        var player = new Player { VisitorId = visitorId };
        playerRepositoryMock.Setup(repository => repository.FindPlayerByVisitorIdAsync(visitorId))
            .ReturnsAsync((Player?)default);
        playerRepositoryMock.Setup(repository => repository.CreatePlayerAsync(It.IsAny<Player>()))
            .ReturnsAsync(player);
        var monitor = playerService.Monitor();

        // act
        player = await playerService.AddPlayerAsync(visitorId, connectionId);

        // assert
        player.VisitorId.Should().Be(visitorId);
        player.RegistrationCount.Should().BeGreaterThan(0);
        playerService.PlayerIds.Should().Contain(player.Id);
        playerService.PlayerCount.Should().BeGreaterThan(0);
        monitor.Should().Raise(nameof(IPlayerService.PlayerAdded));
    }
}