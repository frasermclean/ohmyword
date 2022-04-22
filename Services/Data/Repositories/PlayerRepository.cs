using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using OhMyWord.Core.Models;

namespace OhMyWord.Services.Data.Repositories;

public interface IPlayerRepository
{
    Task<Player> CreatePlayerAsync(Player player);
    Task DeletePlayerAsync(Player player);
    Task<Player?> FindPlayerByPlayerIdAsync(string playerId);
    Task<Player?> FindPlayerByVisitorIdAsync(string visitorId);
    Task<Player?> FindPlayerByConnectionIdAsync(string connectionId);
    Task UpdatePlayerConnectionIdAsync(string playerId, string connectionId);
    Task IncrementPlayerScoreAsync(string playerId, long value);
}

public class PlayerRepository : Repository<Player>, IPlayerRepository
{
    private readonly ILogger<PlayerRepository> logger;

    public PlayerRepository(ICosmosDbService cosmosDbService, ILogger<PlayerRepository> logger)
        : base(cosmosDbService, logger, "Players")
    {
        this.logger = logger;
    }

    public async Task<Player> CreatePlayerAsync(Player player)
    {
        var result = await CreateItemAsync(player);
        return result.Resource ?? throw new InvalidOperationException("Could not create player");
    }

    public Task DeletePlayerAsync(Player player) => DeleteItemAsync(player);

    public async Task<Player?> FindPlayerByPlayerIdAsync(string playerId)
    {
        var result = await ReadItemAsync(playerId, playerId);
        return result.Resource;
    }

    public async Task<Player?> FindPlayerByVisitorIdAsync(string visitorId)
    {
        var queryDefinition = new QueryDefinition("SELECT * FROM c WHERE c.visitorId = @visitorId")
            .WithParameter("@visitorId", visitorId);

        var results = await ExecuteQueryAsync<Player>(queryDefinition);
        return results.FirstOrDefault();
    }

    public async Task<Player?> FindPlayerByConnectionIdAsync(string connectionId)
    {
        var queryDefinition = new QueryDefinition("SELECT * FROM c WHERE c.connectionId = @connectionId")
            .WithParameter("@connectionId", connectionId);

        var results = await ExecuteQueryAsync<Player>(queryDefinition);
        return results.FirstOrDefault();
    }

    public async Task UpdatePlayerConnectionIdAsync(string playerId, string connectionId)
    {
        var patchOperations = new [] { PatchOperation.Set("/connectionId", connectionId) };
        var result = await PatchItemAsync(playerId, playerId, patchOperations);
        if (result.Success)
            logger.LogDebug("Updated player with ID: {playerId} to have connection ID: {connectionId}", playerId, connectionId);
    }

    public async Task IncrementPlayerScoreAsync(string playerId, long value)
    {
        var patchOperations = new[] { PatchOperation.Increment("/score", value) };
        await PatchItemAsync(playerId, playerId, patchOperations);
    }
}
