using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using OhMyWord.Data.Entities;
using OhMyWord.Data.Extensions;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OhMyWord.Data.Services;

public abstract class Repository<TEntity> where TEntity : Entity
{
    private readonly Container container;
    private readonly ILogger<Repository<TEntity>> logger;
    private readonly string entityTypeName;

    private readonly JsonSerializerOptions serializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
    };

    protected Repository(IDatabaseService databaseService, ILogger<Repository<TEntity>> logger, string containerId)
    {
        container = databaseService.GetContainer(containerId);
        this.logger = logger;
        entityTypeName = typeof(TEntity).Name;
    }

    protected async Task<TEntity> CreateItemAsync(TEntity item, CancellationToken cancellationToken = default)
    {
        var response = await container.CreateItemAsync(item, new PartitionKey(item.GetPartition()),
            cancellationToken: cancellationToken);

        logger.LogInformation("Created {TypeName} on partition: /{Partition}, request charge: {Charge} RU",
            entityTypeName, item.GetPartition(), response.RequestCharge);

        return response.Resource;
    }

    protected async Task<TEntity?> ReadItemAsync(string id, string partition,
        CancellationToken cancellationToken = default)
    {
        using var response = await container.ReadItemStreamAsync(id, new PartitionKey(partition),
            cancellationToken: cancellationToken);

        return response.IsSuccessStatusCode
            ? await JsonSerializer.DeserializeAsync<TEntity>(response.Content, serializerOptions, cancellationToken)
            : default;
    }

    protected async Task<TEntity> UpdateItemAsync(TEntity item,
        CancellationToken cancellationToken = default)
    {
        var response = await container.ReplaceItemAsync(item, item.Id, new PartitionKey(item.GetPartition()),
            cancellationToken: cancellationToken);

        logger.LogInformation("Replaced {TypeName} on partition: /{Partition}, request charge: {Charge} RU",
            entityTypeName, item.GetPartition(), response.RequestCharge);

        return response.Resource;
    }

    protected Task DeleteItemAsync(TEntity item) => DeleteItemAsync(item.Id, item.GetPartition());

    protected async Task DeleteItemAsync(string id, string partition, CancellationToken cancellationToken = default)
    {
        var response = await container.DeleteItemAsync<TEntity>(id, new PartitionKey(partition),
            cancellationToken: cancellationToken);

        logger.LogInformation("Deleted {TypeName} on partition: /{Partition}, request charge: {Charge} RU",
            entityTypeName, partition, response.RequestCharge);
    }

    protected async Task<TEntity> PatchItemAsync(string id, string partition, PatchOperation[] operations,
        CancellationToken cancellationToken = default)
    {
        var response = await container.PatchItemAsync<TEntity>(id, new PartitionKey(partition), operations,
            cancellationToken: cancellationToken);

        logger.LogInformation("Patched {TypeName} on partition: /{Partition}, request charge: {Charge} RU",
            entityTypeName, partition, response.RequestCharge);

        return response.Resource;
    }

    protected async Task<int> GetItemCountAsync(string? partition = null, CancellationToken cancellationToken = default)
    {
        QueryDefinition queryDefinition = new("SELECT COUNT(1) FROM c");
        return await ExecuteQueryAsync<CountResponse>(queryDefinition, partition, cancellationToken)
            .CountAsync(cancellationToken);
    }

    /// <summary>
    /// Read all items across all partitions. This is an expensive operation and should be avoided if possible.
    /// </summary>
    protected IAsyncEnumerable<TEntity> ReadAllItemsAsync(CancellationToken cancellationToken)
    {
        QueryDefinition queryDefinition = new("SELECT * FROM c");
        return ExecuteQueryAsync<TEntity>(queryDefinition, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Read all items on the specified partition.
    /// </summary>
    /// <param name="partition">The partition to query items on.</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns></returns>
    protected IAsyncEnumerable<TEntity> ReadPartitionItemsAsync(string partition,
        CancellationToken cancellationToken = default)
    {
        QueryDefinition queryDefinition = new("SELECT * FROM c");
        return ExecuteQueryAsync<TEntity>(queryDefinition, partition, cancellationToken);
    }

    protected IAsyncEnumerable<TResponse> ExecuteQueryAsync<TResponse>(
        QueryDefinition queryDefinition,
        string? partition = null,
        CancellationToken cancellationToken = default)
    {
        using var iterator = container.GetItemQueryIterator<TResponse>(queryDefinition,
            requestOptions: new QueryRequestOptions
            {
                PartitionKey = partition is not null ? new PartitionKey(partition) : null,
            });

        logger.LogInformation("Executing SQL query: {QueryText}, on partition: {Partition}", queryDefinition.QueryText,
            partition);

        return iterator.ToAsyncEnumerable(cancellationToken);
    }
}
