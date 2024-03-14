using OhMyWord.Core.Models;
using OhMyWord.Core.Services;
using OhMyWord.Domain.Services;
using System.Net;
using Microsoft.FeatureManagement;
using OhMyWord.Domain.Options;
using OhMyWord.Integrations.CosmosDb.Errors;
using OhMyWord.Integrations.CosmosDb.Models.Entities;
using OhMyWord.Integrations.CosmosDb.Services;
using OhMyWord.Integrations.GraphApi.Services;
using User = Microsoft.Graph.Models.User;

namespace OhMyWord.Domain.Tests.Services;

[Trait("Category", "Unit")]
public class PlayerServiceTests
{
    private readonly IPlayerService playerService;
    private readonly Mock<IPlayerRepository> playerRepositoryMock = new();
    private readonly Mock<IGraphApiClient> graphApiClientMock = new();
    private readonly Mock<IGeoLocationService> geoLocationServiceMock = new();
    private readonly Mock<IFeatureManager> featureManagerMock = new();

    public PlayerServiceTests()
    {
        playerService = new PlayerService(playerRepositoryMock.Object, graphApiClientMock.Object,
            geoLocationServiceMock.Object, featureManagerMock.Object);
    }

    [Theory, AutoData]
    public async Task GetPlayerAsync_With_NewPlayerId_Should_ReturnExpected(Guid playerId, string visitorId,
        string connectionId, IPAddress ipAddress, Guid? userId, string name, GeoLocation geoLocation)
    {
        // arrange
        playerRepositoryMock.Setup(repository =>
                repository.GetPlayerByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, CancellationToken _) => new ItemNotFoundError(id.ToString(), id.ToString()));

        playerRepositoryMock.Setup(repository =>
                repository.CreatePlayerAsync(It.IsAny<PlayerEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PlayerEntity entity, CancellationToken _) => entity);

        graphApiClientMock.Setup(client => client.GetUserByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, CancellationToken _) => new User { Id = id.ToString(), GivenName = name });

        geoLocationServiceMock.Setup(service =>
                service.GetGeoLocationAsync(It.IsAny<IPAddress>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(geoLocation);
        SetupFeatureManagerMock(true);

        // act
        var player = await playerService.GetPlayerAsync(playerId, visitorId, connectionId, ipAddress, userId);

        // assert
        player.Should().NotBeNull();
        player.Id.Should().Be(playerId);
        player.Name.Should().Be(name);
        player.ConnectionId.Should().Be(connectionId);
        player.UserId.Should().Be(userId);
        player.Score.Should().Be(0);
        player.VisitorId.Should().Be(visitorId);
        player.RegistrationCount.Should().Be(1);
        player.GeoLocation.Should().Be(geoLocation);
    }

    [Theory, AutoData]
    public async Task GetPlayerAsync_With_ExistingPlayerId_Should_ReturnExpected(Guid playerId, string visitorId,
        string connectionId, IPAddress ipAddress, Guid? userId, string name, GeoLocation geoLocation)
    {
        // arrange
        playerRepositoryMock.Setup(repository =>
                repository.GetPlayerByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, CancellationToken _) => new PlayerEntity
            {
                Id = id.ToString(),
                UserId = userId,
                RegistrationCount = 3,
                Score = 123,
                VisitorIds = new[] { visitorId },
                IpAddresses = new[] { ipAddress.ToString() },
            });

        playerRepositoryMock.Setup(repository =>
                repository.UpdatePlayerAsync(It.IsAny<PlayerEntity>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((PlayerEntity entity, string _, string _) => entity with
            {
                RegistrationCount = entity.RegistrationCount + 1
            });

        graphApiClientMock.Setup(client => client.GetUserByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, CancellationToken _) => new User { Id = id.ToString(), GivenName = name });

        geoLocationServiceMock.Setup(service =>
                service.GetGeoLocationAsync(It.IsAny<IPAddress>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(geoLocation);
        SetupFeatureManagerMock(true);


        // act
        var player = await playerService.GetPlayerAsync(playerId, visitorId, connectionId, ipAddress, userId);

        // assert
        player.Should().NotBeNull();
        player.Id.Should().Be(playerId);
        player.Name.Should().Be(name);
        player.ConnectionId.Should().Be(connectionId);
        player.UserId.Should().Be(userId);
        player.Score.Should().Be(123);
        player.RegistrationCount.Should().Be(4);
        player.VisitorId.Should().Be(visitorId);
        player.GeoLocation.Should().Be(geoLocation);
    }

    private void SetupFeatureManagerMock(bool isFeatureEnabled)
    {
        featureManagerMock.Setup(manager => manager.IsEnabledAsync(FeatureFlags.PlayerGeoLocation))
            .ReturnsAsync(isFeatureEnabled);
    }
}