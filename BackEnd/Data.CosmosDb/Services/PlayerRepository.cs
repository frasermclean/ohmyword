using FluentResults;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OhMyWord.Core.Models;
using OhMyWord.Core.Services;
using OhMyWord.Data.CosmosDb.Models;
using OhMyWord.Data.CosmosDb.Options;

namespace OhMyWord.Data.CosmosDb.Services;

public class PlayerRepository : Repository<PlayerItem>, IPlayerRepository
{
    public PlayerRepository(CosmosClient cosmosClient, IOptions<CosmosDbOptions> options,
        ILogger<PlayerRepository> logger)
        : base(cosmosClient, logger, options.Value.DatabaseId, "players")
    {
    }

    public async Task<Result<Player>> GetPlayerAsync(Guid playerId, CancellationToken cancellationToken)
    {
        var result = await GetPlayerItemAsync(playerId, cancellationToken);
        return result.Map(MapToPlayer);
    }

    public async Task<Result<Player>> CreatePlayerAsync(Player player, CancellationToken cancellationToken)
    {
        var item = MapToItem(player);
        var result = await CreateItemAsync(item, cancellationToken);
        return result.Map(MapToPlayer);
    }

    public async Task<Result> DeletePlayerAsync(Guid playerId, CancellationToken cancellationToken)
    {
        var id = playerId.ToString();
        return await DeleteItemAsync(id, id, cancellationToken);
    }

    public async Task<Result<Player>> UpdatePlayerAsync(Guid playerId, string visitorId, string ipAddress,
        CancellationToken cancellationToken)
    {
        var itemResult = await GetPlayerItemAsync(playerId, cancellationToken);
        if (itemResult.IsFailed)
            return itemResult.ToResult();

        var operations = new List<PatchOperation> { PatchOperation.Increment("/registrationCount", 1) };

        // visitor id
        if (!itemResult.Value.VisitorIds.Contains(visitorId))
            operations.Add(PatchOperation.Add("/visitorIds/-", visitorId));

        // ip address
        if (!itemResult.Value.IpAddresses.Contains(ipAddress))
            operations.Add(PatchOperation.Add("/ipAddresses/-", ipAddress));

        return await ApplyPatchOperationsAsync(playerId.ToString(), operations);
    }

    public Task<Result<Player>> IncrementScoreAsync(Guid playerId, long value)
        => ApplyPatchOperationsAsync(playerId.ToString(), new[] { PatchOperation.Increment("/score", value) });

    private async Task<Result<Player>> ApplyPatchOperationsAsync(string playerId,
        IReadOnlyList<PatchOperation> operations)
    {
        var item = await PatchItemAsync(playerId, playerId, operations);
        return MapToPlayer(item);
    }


    private async Task<Result<PlayerItem>> GetPlayerItemAsync(Guid playerId, CancellationToken cancellationToken)
    {
        var id = playerId.ToString();
        return await ReadItemAsync(id, id, cancellationToken);
    }

    private static Player MapToPlayer(PlayerItem item) => new()
    {
        Id = Guid.TryParse(item.Id, out var id) ? id : Guid.Empty,
        UserId = item.UserId,
        Score = item.Score,
        RegistrationCount = item.RegistrationCount
    };

    private static PlayerItem MapToItem(Player player) => new()
    {
        Id = player.Id.ToString(),
        UserId = player.UserId,
        Score = player.Score,
        RegistrationCount = player.RegistrationCount,
    };
}
