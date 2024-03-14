using OhMyWord.Core.Models;
using OhMyWord.Core.Services;
using System.Net;
using Microsoft.FeatureManagement;
using OhMyWord.Domain.Options;
using OhMyWord.Integrations.CosmosDb.Models.Entities;
using OhMyWord.Integrations.CosmosDb.Services;
using OhMyWord.Integrations.GraphApi.Services;

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
    private readonly IFeatureManager featureManager;

    public PlayerService(IPlayerRepository playerRepository, IGraphApiClient graphApiClient,
        IGeoLocationService geoLocationService, IFeatureManager featureManager)
    {
        this.playerRepository = playerRepository;
        this.graphApiClient = graphApiClient;
        this.geoLocationService = geoLocationService;
        this.featureManager = featureManager;
    }

    public async Task<Player> GetPlayerAsync(Guid playerId, string visitorId, string connectionId, IPAddress ipAddress,
        Guid? userId = default, CancellationToken cancellationToken = default)
    {
        var entityTask = GetOrCreatePlayerEntityAsync(playerId, visitorId, ipAddress, userId, cancellationToken);
        var nameTask = GetPlayerNameAsync(userId, cancellationToken);
        var geoLocationTask = GetGeoLocationAsync(ipAddress, cancellationToken);

        await Task.WhenAll(entityTask, nameTask, geoLocationTask);

        return MapToPlayer(entityTask.Result, nameTask.Result, visitorId, connectionId, geoLocationTask.Result);
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

    private async Task<GeoLocation> GetGeoLocationAsync(IPAddress ipAddress, CancellationToken cancellationToken)
    {
        var isFeatureEnabled = await featureManager.IsEnabledAsync(FeatureFlags.PlayerGeoLocation);

        if (!isFeatureEnabled)
            return GeoLocation.Default;

        var result = await geoLocationService.GetGeoLocationAsync(ipAddress, cancellationToken);
        return result.IsSuccess ? result.Value : GeoLocation.Default;
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