using FluentResults;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OhMyWord.Integrations.CosmosDb.Models.Entities;
using OhMyWord.Integrations.CosmosDb.Options;

namespace OhMyWord.Integrations.CosmosDb.Services;

public interface IPlayerRepository
{
    Task<Result<PlayerEntity>> GetPlayerByIdAsync(Guid playerId, CancellationToken cancellationToken = default);
    Task<PlayerEntity> CreatePlayerAsync(PlayerEntity playerEntity, CancellationToken cancellationToken = default);
    Task DeletePlayerAsync(PlayerEntity playerEntity);
    Task<PlayerEntity> UpdatePlayerAsync(PlayerEntity entity, string visitorId, string ipAddress);
    Task<PlayerEntity> IncrementScoreAsync(Guid playerId, long value);
}

public class PlayerRepository : Repository<PlayerEntity>, IPlayerRepository
{
    public PlayerRepository(CosmosClient cosmosClient, IOptions<CosmosDbOptions> options,
        ILogger<PlayerRepository> logger)
        : base(cosmosClient, logger, options.Value.DatabaseId, "players")
    {
    }

    public Task<Result<PlayerEntity>> GetPlayerByIdAsync(Guid playerId, CancellationToken cancellationToken)
    {
        var id = playerId.ToString();
        return ReadItemAsync(id, id, cancellationToken);
    }

    public async Task<PlayerEntity> CreatePlayerAsync(PlayerEntity playerEntity, CancellationToken cancellationToken)
    {
        await CreateItemAsync(playerEntity, cancellationToken);
        return playerEntity;
    }

    public Task DeletePlayerAsync(PlayerEntity playerEntity) => DeleteItemAsync(playerEntity);

    public Task<PlayerEntity> UpdatePlayerAsync(PlayerEntity entity, string visitorId, string ipAddress)
    {
        var operations = new List<PatchOperation> { PatchOperation.Increment("/registrationCount", 1) };

        // visitor id
        if (!entity.VisitorIds.Contains(visitorId))
            operations.Add(PatchOperation.Add("/visitorIds/-", visitorId));

        // ip address
        if (!entity.IpAddresses.Contains(ipAddress))
            operations.Add(PatchOperation.Add("/ipAddresses/-", ipAddress));

        return ApplyPatchOperationsAsync(entity.Id, operations);
    }

    public Task<PlayerEntity> IncrementScoreAsync(Guid playerId, long value)
        => ApplyPatchOperationsAsync(playerId.ToString(), new[] { PatchOperation.Increment("/score", value) });

    private Task<PlayerEntity> ApplyPatchOperationsAsync(string playerId, IReadOnlyList<PatchOperation> operations)
        => PatchItemAsync(playerId, playerId, operations);
}
