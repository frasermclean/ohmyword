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
    private readonly IPlayerRepository playerRepository;
    private readonly IGraphApiClient graphApiClient;

    public PlayerService(IPlayerRepository playerRepository, IGraphApiClient graphApiClient)
    {
        this.playerRepository = playerRepository;
        this.graphApiClient = graphApiClient;
    }

    public async Task<Player> GetPlayerAsync(Guid playerId, string visitorId, string connectionId,
        IPAddress ipAddress, Guid? userId = default, CancellationToken cancellationToken = default)
    {
        var entityTask = GetOrCreatePlayerEntityAsync(playerId, visitorId, connectionId, ipAddress,
            userId, cancellationToken);
        var nameTask = GetPlayerNameAsync(userId, cancellationToken);

        await Task.WhenAll(entityTask, nameTask);

        return MapToPlayer(entityTask.Result, nameTask.Result, visitorId, connectionId, ipAddress);
    }

    public Task IncrementPlayerScoreAsync(Guid playerId, int points) =>
        playerRepository.IncrementScoreAsync(playerId, points);

    private async Task<PlayerEntity> GetOrCreatePlayerEntityAsync(Guid playerId, string visitorId, string connectionId,
        IPAddress ipAddress, Guid? userId = default, CancellationToken cancellationToken = default)
    {
        var entity = await playerRepository.GetPlayerByIdAsync(playerId);
        if (entity is null)
        {
            // create a new player entity
            return await playerRepository.CreatePlayerAsync(new PlayerEntity
            {
                Id = playerId.ToString(),
                UserId = userId,
                VisitorIds = new[] { visitorId },
                IpAddresses = new[] { ipAddress.ToString() }
            });
        }

        // update existing player entity
        await playerRepository.IncrementRegistrationCountAsync(playerId);

        // patch ip address
        if (!entity.IpAddresses.Contains(ipAddress.ToString()))
            await playerRepository.AddIpAddressAsync(playerId, ipAddress.ToString());

        // patch visitor id
        if (!entity.VisitorIds.Contains(visitorId))
            await playerRepository.AddVisitorIdAsync(playerId, visitorId);

        return entity;
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
