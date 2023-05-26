using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OhMyWord.Infrastructure.Models.Entities;
using OhMyWord.Infrastructure.Options;

namespace OhMyWord.Infrastructure.Services;

public interface IPlayerRepository
{
    Task<PlayerEntity?> GetPlayerByIdAsync(Guid playerId);
    Task<PlayerEntity> CreatePlayerAsync(PlayerEntity playerEntity);
    Task DeletePlayerAsync(PlayerEntity playerEntity);
    Task<PlayerEntity> IncrementRegistrationCountAsync(Guid playerId);
    Task<PlayerEntity> IncrementScoreAsync(Guid playerId, long value);
    Task AddIpAddressAsync(Guid playerId, string ipAddress);
    Task AddVisitorIdAsync(Guid playerId, string visitorId);
}

public class PlayerRepository : Repository<PlayerEntity>, IPlayerRepository
{
    public PlayerRepository(CosmosClient cosmosClient, IOptions<CosmosDbOptions> options,
        ILogger<PlayerRepository> logger)
        : base(cosmosClient, options, logger, "players")
    {
    }

    public Task<PlayerEntity?> GetPlayerByIdAsync(Guid playerId)
    {
        var id = playerId.ToString();
        return ReadItemAsync(id, id);
    }

    public async Task<PlayerEntity> CreatePlayerAsync(PlayerEntity playerEntity)
    {
        await CreateItemAsync(playerEntity);
        return playerEntity;
    }

    public Task DeletePlayerAsync(PlayerEntity playerEntity) => DeleteItemAsync(playerEntity);

    public Task<PlayerEntity> IncrementRegistrationCountAsync(Guid playerId)
    {
        var id = playerId.ToString();
        return PatchItemAsync(id, id, new[] { PatchOperation.Increment("/registrationCount", 1) });
    }

    public Task<PlayerEntity> IncrementScoreAsync(Guid playerId, long value)
    {
        var id = playerId.ToString();
        return PatchItemAsync(id, id, new[] { PatchOperation.Increment("/score", value) });
    }

    public Task AddIpAddressAsync(Guid playerId, string ipAddress)
    {
        var id = playerId.ToString();
        return PatchItemAsync(id, id, new[] { PatchOperation.Add("/ipAddresses/-", ipAddress) });
    }

    public Task AddVisitorIdAsync(Guid playerId, string visitorId)
    {
        var id = playerId.ToString();
        return PatchItemAsync(id, id, new[] { PatchOperation.Add("/visitorIds/-", visitorId) });
    }
}
