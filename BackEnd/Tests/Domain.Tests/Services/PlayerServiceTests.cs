using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Graph.Models;
using OhMyWord.Domain.Services;
using OhMyWord.Infrastructure.Models.Entities;
using OhMyWord.Infrastructure.Services;
using OhMyWord.Infrastructure.Services.GraphApi;
using System.Net;

namespace Domain.Tests.Services;

[Trait("Category", "Unit")]
public class PlayerServiceTests
{
    private readonly IPlayerService playerService;
    private readonly Mock<IPlayerRepository> playerRepositoryMock = new();
    private readonly Mock<IGraphApiClient> graphApiClientMock = new();

    public PlayerServiceTests()
    {
        var serviceProvider = new ServiceCollection()
            .AddMemoryCache()
            .BuildServiceProvider();

        var memoryCache = serviceProvider.GetRequiredService<IMemoryCache>();

        playerService =
            new PlayerService(memoryCache, playerRepositoryMock.Object, graphApiClientMock.Object);
    }

    [Theory, AutoData]
    public async Task GetPlayerAsync_With_NewPlayerId_Should_ReturnExpected(Guid playerId, string visitorId,
        string connectionId, IPAddress ipAddress, Guid? userId, string name)
    {
        // arrange
        playerRepositoryMock.Setup(repository =>
                repository.GetPlayerByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(null as PlayerEntity);
        
        playerRepositoryMock.Setup(repository =>
                repository.CreatePlayerAsync(It.IsAny<PlayerEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PlayerEntity entity, CancellationToken _) => entity);

        graphApiClientMock.Setup(client => client.GetUserByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, CancellationToken _) => new User { Id = id.ToString(), GivenName = name });

        // act
        var player = await playerService.GetPlayerAsync(playerId, visitorId, connectionId, ipAddress, userId);

        // assert
        player.Should().NotBeNull();
        player.Id.Should().Be(playerId);
        player.Name.Should().Be(name);
        player.ConnectionId.Should().Be(connectionId);
        player.UserId.Should().Be(userId);
        player.VisitorId.Should().Be(visitorId);
        player.RegistrationCount.Should().Be(1);
    }

    [Theory, AutoData]
    public async Task GetPlayerAsync_With_ExistingPlayerId_Should_ReturnExpected(Guid playerId, string visitorId,
        string connectionId, IPAddress ipAddress, Guid? userId, string name)
    {
        // arrange
        playerRepositoryMock.Setup(repository =>
                repository.GetPlayerByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, CancellationToken _) => new PlayerEntity
            {
                Id = id.ToString(),
                UserId = userId,
                VisitorIds = new[] { visitorId },
                IpAddresses = new[] { ipAddress.ToString() },
            });

        graphApiClientMock.Setup(client => client.GetUserByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, CancellationToken _) => new User { Id = id.ToString(), GivenName = name });

        // act
        var player = await playerService.GetPlayerAsync(playerId, visitorId, connectionId, ipAddress, userId);

        // assert
        player.Should().NotBeNull();
        player.Id.Should().Be(playerId);
        player.Name.Should().Be(name);
        player.ConnectionId.Should().Be(connectionId);
        player.UserId.Should().Be(userId);
        player.VisitorId.Should().Be(visitorId);
    }
}
