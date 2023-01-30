using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using OhMyWord.Data.Entities;

namespace OhMyWord.Data.Services;

public interface IPlayerRepository
{
    Task<PlayerEntity> CreatePlayerAsync(PlayerEntity playerEntity);
    Task DeletePlayerAsync(PlayerEntity playerEntity);
    Task<PlayerEntity?> FindPlayerByPlayerIdAsync(string playerId);
    Task<PlayerEntity?> FindPlayerByVisitorIdAsync(string visitorId);
    Task<PlayerEntity> IncrementPlayerRegistrationCountAsync(string playerId);
    Task<PlayerEntity> IncrementPlayerScoreAsync(string playerId, long value);
}

public class PlayerRepository : Repository<PlayerEntity>, IPlayerRepository
{
    public PlayerRepository(IDatabaseService databaseService, ILogger<PlayerRepository> logger)
        : base(databaseService, logger, "players")
    {
    }

    public async Task<PlayerEntity> CreatePlayerAsync(PlayerEntity playerEntity) => await CreateItemAsync(playerEntity);

    public Task DeletePlayerAsync(PlayerEntity playerEntity) => DeleteItemAsync(playerEntity);

    public async Task<PlayerEntity?> FindPlayerByPlayerIdAsync(string playerId) =>
        await ReadItemAsync(playerId, playerId);

    public async Task<PlayerEntity?> FindPlayerByVisitorIdAsync(string visitorId)
    {
        var queryDefinition = new QueryDefinition("SELECT * FROM c WHERE c.visitorId = @visitorId")
            .WithParameter("@visitorId", visitorId);

        return await ExecuteQueryAsync<PlayerEntity>(queryDefinition).FirstOrDefaultAsync();
    }

    public async Task<PlayerEntity> IncrementPlayerRegistrationCountAsync(string playerId)
    {
        return await PatchItemAsync(playerId, playerId, new[] { PatchOperation.Increment("/registrationCount", 1) });
    }

    public async Task<PlayerEntity> IncrementPlayerScoreAsync(string playerId, long value)
    {
        return await PatchItemAsync(playerId, playerId, new[] { PatchOperation.Increment("/score", value) });
    }
}
