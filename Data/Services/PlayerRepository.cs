using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using OhMyWord.Data.Models;

namespace OhMyWord.Data.Services;

public interface IPlayerRepository
{
    Task<Player> CreatePlayerAsync(Player player);
    Task DeletePlayerAsync(Player player);
    Task<Player?> FindPlayerByPlayerIdAsync(Guid playerId);
    Task<Player?> FindPlayerByVisitorIdAsync(string visitorId);
    Task<Player> IncrementPlayerRegistrationCountAsync(Guid playerId);
    Task<Player> IncrementPlayerScoreAsync(Guid playerId, long value);
}

public class PlayerRepository : Repository<Player>, IPlayerRepository
{
    public PlayerRepository(ICosmosDbService cosmosDbService, ILogger<PlayerRepository> logger)
        : base(cosmosDbService, logger, "Players")
    {
    }

    public async Task<Player> CreatePlayerAsync(Player player) => await CreateItemAsync(player);

    public Task DeletePlayerAsync(Player player) => DeleteItemAsync(player);

    public async Task<Player?> FindPlayerByPlayerIdAsync(Guid playerId) =>
        await ReadItemAsync(playerId, playerId.ToString());

    public async Task<Player?> FindPlayerByVisitorIdAsync(string visitorId)
    {
        var queryDefinition = new QueryDefinition("SELECT * FROM c WHERE c.visitorId = @visitorId")
            .WithParameter("@visitorId", visitorId);

        var results = await ExecuteQueryAsync<Player>(queryDefinition);
        return results.FirstOrDefault();
    }

    public async Task<Player> IncrementPlayerRegistrationCountAsync(Guid playerId)
    {
        return await PatchItemAsync(playerId, playerId.ToString(),
            new[] { PatchOperation.Increment("/registrationCount", 1) });
    }

    public async Task<Player> IncrementPlayerScoreAsync(Guid playerId, long value)
    {
        return await PatchItemAsync(playerId, playerId.ToString(), new[] { PatchOperation.Increment("/score", value) });
    }
}
