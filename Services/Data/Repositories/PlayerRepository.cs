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
    Task<bool> IncrementPlayerScoreAsync(string playerId, long value);
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

    public async Task<bool> IncrementPlayerScoreAsync(string playerId, long value)
    {
        var patchOperations = new[] { PatchOperation.Increment("/score", value) };
        var result = await PatchItemAsync(playerId, playerId, patchOperations);
        if (!result.Success)
        {
            logger.LogWarning("Could not increment player score. Message: '{message}'", result.Message);
            return false;
        }

        logger.LogDebug("Incremented player with ID: {playerId} score by: {value}", playerId, value);
        return true;
    }
}
