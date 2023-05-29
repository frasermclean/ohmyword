using Microsoft.Extensions.Caching.Memory;
using OhMyWord.Domain.Models;
using OhMyWord.Infrastructure.Models.Entities;
using OhMyWord.Infrastructure.Services;
using OhMyWord.Infrastructure.Services.GraphApi;
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
    private readonly IMemoryCache memoryCache;
    private readonly IPlayerRepository playerRepository;
    private readonly IGraphApiClient graphApiClient;

    public PlayerService(IMemoryCache memoryCache, IPlayerRepository playerRepository, IGraphApiClient graphApiClient)
    {
        this.memoryCache = memoryCache;
        this.playerRepository = playerRepository;
        this.graphApiClient = graphApiClient;
    }

    public async Task<Player> GetPlayerAsync(Guid playerId, string visitorId, string connectionId, IPAddress ipAddress,
        Guid? userId = default, CancellationToken cancellationToken = default)
    {
        var player = await memoryCache.GetOrCreateAsync<Player>($"Player-{playerId}", async entry =>
        {
            var entityTask = GetOrCreatePlayerEntityAsync(playerId, visitorId, ipAddress, userId, cancellationToken);
            var nameTask = GetPlayerNameAsync(userId, cancellationToken);

            await Task.WhenAll(entityTask, nameTask);

            var player = MapToPlayer(entityTask.Result, nameTask.Result, visitorId, connectionId, ipAddress);

            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);

            return player;
        });

        return player ?? throw new InvalidOperationException("Memory cache failed to return a player.");
    }

    public Task IncrementPlayerScoreAsync(Guid playerId, int points) =>
        playerRepository.IncrementScoreAsync(playerId, points);

    private async Task<PlayerEntity> GetOrCreatePlayerEntityAsync(Guid playerId, string visitorId, IPAddress ipAddress,
        Guid? userId = default, CancellationToken cancellationToken = default)
    {
        var entity = await playerRepository.GetPlayerByIdAsync(playerId, cancellationToken);
        if (entity is null)
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
        return await playerRepository.UpdatePlayerAsync(entity, visitorId, ipAddress.ToString());        
    }    

    private async Task<string> GetPlayerNameAsync(Guid? userId, CancellationToken cancellationToken = default)
    {
        if (!userId.HasValue) return "Guest";

        var user = await graphApiClient.GetUserByIdAsync(userId.Value, cancellationToken);
        return user?.GivenName ?? "Guest";
    }

    private static Player MapToPlayer(PlayerEntity entity, string name, string visitorId, string connectionId,
        IPAddress ipAddress) => new()
    {
        Id = Guid.TryParse(entity.Id, out var id) ? id : Guid.Empty,
        Name = name,
        ConnectionId = connectionId,
        UserId = entity.UserId,
        Score = entity.Score,
        RegistrationCount = entity.RegistrationCount,
        VisitorId = visitorId,
        IpAddress = ipAddress
    };
}
