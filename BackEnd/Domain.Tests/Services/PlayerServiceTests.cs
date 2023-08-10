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
        string connectionId, IPAddress ipAddress, Guid? userId, string name, GeoLocation geoLocation)
    {
        // arrange
        playerRepository.GetPlayerByIdAsync(playerId, Arg.Any<CancellationToken>())
            .Returns(info => new ItemNotFoundError(info.Arg<Guid>().ToString(), info.Arg<Guid>().ToString()));

        playerRepository.CreatePlayerAsync(Arg.Any<PlayerEntity>(), Arg.Any<CancellationToken>())
            .Returns(info => info.Arg<PlayerEntity>());

        graphApiClient.GetUserByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(info => new User { Id = info.Arg<Guid>().ToString(), GivenName = name });

        geoLocationService.GetGeoLocationAsync(Arg.Any<IPAddress>(), Arg.Any<CancellationToken>())
            .Returns(geoLocation);

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
}

//     [Theory, AutoData]
//     public async Task GetPlayerAsync_With_ExistingPlayerId_Should_ReturnExpected(Guid playerId, string visitorId,
//         string connectionId, IPAddress ipAddress, Guid? userId, string name, GeoLocation geoLocation)
//     {
//         // arrange
//         playerRepositoryMock.Setup(repository =>
//                 repository.GetPlayerByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
//             .ReturnsAsync((Guid id, CancellationToken _) => new PlayerEntity
//             {
//                 Id = id.ToString(),
//                 UserId = userId,
//                 RegistrationCount = 3,
//                 Score = 123,
//                 VisitorIds = new[] { visitorId },
//                 IpAddresses = new[] { ipAddress.ToString() },
//             });
//
//         playerRepositoryMock.Setup(repository =>
//                 repository.UpdatePlayerAsync(It.IsAny<PlayerEntity>(), It.IsAny<string>(), It.IsAny<string>()))
//             .ReturnsAsync((PlayerEntity entity, string _, string _) => entity with
//             {
//                 RegistrationCount = entity.RegistrationCount + 1
//             });
//
//         graphApiClientMock.Setup(client => client.GetUserByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
//             .ReturnsAsync((Guid id, CancellationToken _) => new User { Id = id.ToString(), GivenName = name });
//
//         geoLocationServiceMock.Setup(service =>
//                 service.GetGeoLocationAsync(It.IsAny<IPAddress>(), It.IsAny<CancellationToken>()))
//             .ReturnsAsync(geoLocation);
//
//         // act
//         var player = await playerService.GetPlayerAsync(playerId, visitorId, connectionId, ipAddress, userId);
//
//         // assert
//         player.Should().NotBeNull();
//         player.Id.Should().Be(playerId);
//         player.Name.Should().Be(name);
//         player.ConnectionId.Should().Be(connectionId);
//         player.UserId.Should().Be(userId);
//         player.Score.Should().Be(123);
//         player.RegistrationCount.Should().Be(4);
//         player.VisitorId.Should().Be(visitorId);
//         player.GeoLocation.Should().Be(geoLocation);
//     }
// }
