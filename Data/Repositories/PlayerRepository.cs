using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using OhMyWord.Data.Models;

namespace OhMyWord.Data.Repositories;

public interface IPlayerRepository
{
    Task<Player> CreatePlayerAsync(Player player);
    Task DeletePlayerAsync(string playerId);
    Task<Player?> FindPlayerByVisitorIdAsync(string visitorId);
    Task<Player?> FindPlayerByConnectionIdAsync(string connectionId);
    Task<Player> UpdatePlayerConnectionIdAsync(Player player, string connectionId);
}

public class PlayerRepository : Repository<Player>, IPlayerRepository
{
    public PlayerRepository(ICosmosDbService cosmosDbService, ILogger<PlayerRepository> logger)
        : base(cosmosDbService, logger)
    {
    }

    public Task<Player> CreatePlayerAsync(Player player) => CreateItemAsync(player);
    public Task DeletePlayerAsync(string playerId) => DeleteItemAsync(playerId);

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

    public Task<Player> UpdatePlayerConnectionIdAsync(Player player, string connectionId)
    {
        throw new NotImplementedException();
    }
}
