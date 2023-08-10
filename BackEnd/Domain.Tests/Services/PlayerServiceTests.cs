using OhMyWord.Core.Models;
using OhMyWord.Domain.Services;
using OhMyWord.Integrations.Errors;
using OhMyWord.Integrations.Models.Entities;
using OhMyWord.Integrations.Services.GraphApi;
using OhMyWord.Integrations.Services.Repositories;
using System.Net;
using User = Microsoft.Graph.Models.User;

namespace OhMyWord.Domain.Tests.Services;

[Trait("Category", "Unit")]
public class PlayerServiceTests
{
    private readonly IPlayerService playerService;
    private readonly IPlayerRepository playerRepository = Substitute.For<IPlayerRepository>();
    private readonly IGraphApiClient graphApiClient = Substitute.For<IGraphApiClient>();
    private readonly IGeoLocationService geoLocationService = Substitute.For<IGeoLocationService>();

    public PlayerServiceTests()
    {
        playerService = new PlayerService(playerRepository, graphApiClient, geoLocationService);
    }

    [Theory, AutoData]
    public async Task GetPlayerAsync_With_NewPlayerId_Should_ReturnExpected(Guid playerId, string visitorId,
        string connectionId, IPAddress ipAddress, Guid userId, string name, GeoLocation geoLocation)
    {
        // arrange
        playerRepository.GetPlayerByIdAsync(playerId, Arg.Any<CancellationToken>())
            .Returns(info => new ItemNotFoundError(info.Arg<Guid>().ToString(), info.Arg<Guid>().ToString()));

        playerRepository.CreatePlayerAsync(Arg.Any<PlayerEntity>(), Arg.Any<CancellationToken>())
            .Returns(info => info.Arg<PlayerEntity>());

        SetupGraphApiClient(userId, name);
        SetupGeoLocationService(ipAddress, geoLocation);

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
        string connectionId, IPAddress ipAddress, Guid userId, string name, GeoLocation geoLocation)
    {
        // arrange
        var playerEntity = new PlayerEntity()
        {
            Id = playerId.ToString(),
            UserId = userId,
            RegistrationCount = 3,
            Score = 123,
            VisitorIds = new[] { visitorId },
            IpAddresses = new[] { ipAddress.ToString() },
        };
        SetupPlayerRepository(playerId, visitorId, ipAddress, playerEntity);
        SetupGraphApiClient(userId, name);
        SetupGeoLocationService(ipAddress, geoLocation);

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

    private void SetupPlayerRepository(Guid playerId, string visitorId, IPAddress ipAddress, PlayerEntity playerEntity)
    {
        playerRepository.GetPlayerByIdAsync(playerId, Arg.Any<CancellationToken>())
            .Returns(playerEntity);

        playerRepository.UpdatePlayerAsync(playerEntity, visitorId, ipAddress.ToString())
            .Returns(playerEntity with { RegistrationCount = playerEntity.RegistrationCount + 1 });
    }

    private void SetupGraphApiClient(Guid userId, string name)
    {
        graphApiClient.GetUserByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(info => new User { Id = userId.ToString(), GivenName = name });
    }

    private void SetupGeoLocationService(IPAddress ipAddress, GeoLocation geoLocation)
    {
        geoLocationService.GetGeoLocationAsync(ipAddress, Arg.Any<CancellationToken>())
            .Returns(geoLocation);
    }
}
