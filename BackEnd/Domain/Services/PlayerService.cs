using OhMyWord.Core.Models;
using OhMyWord.Core.Services;
using OhMyWord.Integrations.Models.Entities;
using OhMyWord.Integrations.Services.GraphApi;
using OhMyWord.Integrations.Services.Repositories;
using System.Net;

namespace OhMyWord.Domain.Services;

public interface IPlayerService
{
    Task<Player> GetPlayerAsync(Guid playerId, string visitorId, string connectionId, IPAddress ipAddress,
        Guid? userId = default, CancellationToken cancellationToken = default);

    Task IncrementPlayerScoreAsync(Guid playerId, int points);
}

public class PlayerService : IPlayerService
{
    private readonly IPlayerRepository playerRepository;
    private readonly IGraphApiClient graphApiClient;
    private readonly IGeoLocationService geoLocationService;

    public PlayerService(IPlayerRepository playerRepository, IGraphApiClient graphApiClient,
        IGeoLocationService geoLocationService)
    {
        this.playerRepository = playerRepository;
        this.graphApiClient = graphApiClient;
        this.geoLocationService = geoLocationService;
    }

    public async Task<Player> GetPlayerAsync(Guid playerId, string visitorId, string connectionId, IPAddress ipAddress,
        Guid? userId = default, CancellationToken cancellationToken = default)
    {
        var entityTask = GetOrCreatePlayerEntityAsync(playerId, visitorId, ipAddress, userId, cancellationToken);
        var nameTask = GetPlayerNameAsync(userId, cancellationToken);
        var geoLocationTask = geoLocationService.GetGeoLocationAsync(ipAddress, cancellationToken);

        await Task.WhenAll(entityTask, nameTask, geoLocationTask);

        return MapToPlayer(entityTask.Result, nameTask.Result, visitorId, connectionId, geoLocationTask.Result.Value);
    }

    public Task IncrementPlayerScoreAsync(Guid playerId, int points) =>
        playerRepository.IncrementScoreAsync(playerId, points);

    private async Task<PlayerEntity> GetOrCreatePlayerEntityAsync(Guid playerId, string visitorId, IPAddress ipAddress,
        Guid? userId = default, CancellationToken cancellationToken = default)
    {
        var result = await playerRepository.GetPlayerByIdAsync(playerId, cancellationToken);
        if (result.IsFailed)
        {
            // create a new player entity
            return await playerRepository.CreatePlayerAsync(
                new PlayerEntity
                {
                    Id = playerId.ToString(),
                    UserId = userId,
                    VisitorIds = new[] { visitorId },
                    IpAddresses = new[] { ipAddress.ToString() }
                }, cancellationToken);
        }

        // update the player entity
        return await playerRepository.UpdatePlayerAsync(result.Value, visitorId, ipAddress.ToString());
    }

    private async Task<string> GetPlayerNameAsync(Guid? userId, CancellationToken cancellationToken = default)
    {
        if (!userId.HasValue) return "Guest";

        var user = await graphApiClient.GetUserByIdAsync(userId.Value, cancellationToken);
        return user?.GivenName ?? "Guest";
    }

    private static Player MapToPlayer(PlayerEntity entity, string name, string visitorId, string connectionId,
        GeoLocation geoLocation) => new()
    {
        Id = Guid.TryParse(entity.Id, out var id) ? id : Guid.Empty,
        Name = name,
        ConnectionId = connectionId,
        UserId = entity.UserId,
        Score = entity.Score,
        RegistrationCount = entity.RegistrationCount,
        VisitorId = visitorId,
        GeoLocation = geoLocation
    };
}
