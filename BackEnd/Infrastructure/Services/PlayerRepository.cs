using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OhMyWord.Infrastructure.Entities;
using OhMyWord.Infrastructure.Options;

namespace OhMyWord.Infrastructure.Services;

public interface IPlayerRepository
{
    Task<PlayerEntity?> GetPlayerByVisitorIdAsync(string visitorId);
    Task<PlayerEntity> CreatePlayerAsync(PlayerEntity playerEntity);
    Task DeletePlayerAsync(PlayerEntity playerEntity);
    Task<PlayerEntity> IncrementRegistrationCountAsync(string playerId);
    Task<PlayerEntity> IncrementScoreAsync(string playerId, long value);
}

public class PlayerRepository : Repository<PlayerEntity>, IPlayerRepository
{
    public PlayerRepository(CosmosClient cosmosClient, IOptions<CosmosDbOptions> options,
        ILogger<PlayerRepository> logger)
        : base(cosmosClient, options, logger, "players")
    {
    }

    public async Task<PlayerEntity?> GetPlayerByVisitorIdAsync(string visitorId)
    {
        const string sql = "SELECT * FROM c WHERE c.visitorId = @visitorId";
        var queryDefinition = new QueryDefinition(sql).WithParameter("@visitorId", visitorId);
        return await ExecuteQuery<PlayerEntity>(queryDefinition).FirstOrDefaultAsync();
    }

    public async Task<PlayerEntity> CreatePlayerAsync(PlayerEntity playerEntity)
    {
        await CreateItemAsync(playerEntity);
        return playerEntity;
    }

    public Task DeletePlayerAsync(PlayerEntity playerEntity) => DeleteItemAsync(playerEntity);

    public Task<PlayerEntity> IncrementRegistrationCountAsync(string playerId) => PatchItemAsync(playerId,
        playerId, new[] { PatchOperation.Increment("/registrationCount", 1) });

    public Task<PlayerEntity> IncrementScoreAsync(string playerId, long value) =>
        PatchItemAsync(playerId, playerId, new[] { PatchOperation.Increment("/score", value) });
}
