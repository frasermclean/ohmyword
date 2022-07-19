using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using OhMyWord.Core.Models;

namespace OhMyWord.Services.Data.Repositories;

public interface IPlayerRepository
{
    Task<Player> CreatePlayerAsync(Player player);
    Task DeletePlayerAsync(Player player);
    Task<Player?> FindPlayerByPlayerIdAsync(Guid playerId);
    Task<Player?> FindPlayerByVisitorIdAsync(string visitorId);
    Task IncrementPlayerRegistrationCountAsync(Guid playerId);
    Task<bool> IncrementPlayerScoreAsync(Guid playerId, long value);
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

    public async Task<Player?> FindPlayerByPlayerIdAsync(Guid playerId)
    {
        var result = await ReadItemAsync(playerId, playerId.ToString());
        return result.Resource;
    }

    public async Task<Player?> FindPlayerByVisitorIdAsync(string visitorId)
    {
        var queryDefinition = new QueryDefinition("SELECT * FROM c WHERE c.visitorId = @visitorId")
            .WithParameter("@visitorId", visitorId);

        var results = await ExecuteQueryAsync<Player>(queryDefinition);
        return results.FirstOrDefault();
    }

    public async Task IncrementPlayerRegistrationCountAsync(Guid playerId)
    {
        var patchOperations = new[] { PatchOperation.Increment("/registrationCount", 1) };
        var result = await PatchItemAsync(playerId, playerId.ToString(), patchOperations);

        if (!result.Success)
            logger.LogWarning("Could not increment player registration count. Message: '{message}'", result.Message);
    }

    public async Task<bool> IncrementPlayerScoreAsync(Guid playerId, long value)
    {
        var patchOperations = new[] { PatchOperation.Increment("/score", value) };
        var result = await PatchItemAsync(playerId, playerId.ToString(), patchOperations);
        if (!result.Success)
        {
            logger.LogWarning("Could not increment player score. Message: '{message}'", result.Message);
            return false;
        }

        logger.LogDebug("Incremented player with ID: {playerId} score by: {value}", playerId, value);
        return true;
    }
}
