using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using OhMyWord.Data.Models;

namespace OhMyWord.Data.Services;

public abstract class Repository<TEntity> where TEntity : Entity
{
    private readonly Container container;
    private readonly ILogger<Repository<TEntity>> logger;
    private readonly string entityTypeName;

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

    protected async Task<TEntity?> ReadItemAsync(Guid id, string partition,
        CancellationToken cancellationToken = default)
    {
        var response = await container.ReadItemAsync<TEntity>(id.ToString(), new PartitionKey(partition),
            cancellationToken: cancellationToken);

        logger.LogInformation("Read {TypeName} on partition: /{Partition}, request charge: {Charge} RU",
            entityTypeName, partition, response.RequestCharge);

        return response.Resource;
    }

    protected async Task<TEntity> UpdateItemAsync(TEntity item,
        CancellationToken cancellationToken = default)
    {
        var response = await container.ReplaceItemAsync(item, item.Id.ToString(), new PartitionKey(item.GetPartition()),
            cancellationToken: cancellationToken);

        logger.LogInformation("Replaced {TypeName} on partition: /{Partition}, request charge: {Charge} RU",
            entityTypeName, item.GetPartition(), response.RequestCharge);

        return response.Resource;
    }

    protected Task DeleteItemAsync(TEntity item) => DeleteItemAsync(item.Id, item.GetPartition());

    protected async Task DeleteItemAsync(Guid id, string partition,
        CancellationToken cancellationToken = default)
    {
        var response = await container.DeleteItemAsync<TEntity>(id.ToString(), new PartitionKey(partition),
            cancellationToken: cancellationToken);

        logger.LogInformation("Deleted {TypeName} on partition: /{Partition}, request charge: {Charge} RU",
            entityTypeName, partition, response.RequestCharge);
    }

    protected async Task<TEntity> PatchItemAsync(
        Guid id,
        string partition,
        PatchOperation[] operations,
        CancellationToken cancellationToken = default)
    {
        var response = await container.PatchItemAsync<TEntity>(id.ToString(), new PartitionKey(partition), operations,
            cancellationToken: cancellationToken);

        logger.LogInformation("Patched {TypeName} on partition: /{Partition}, request charge: {Charge} RU",
            entityTypeName, partition, response.RequestCharge);

        return response.Resource;
    }

    protected async Task<int> GetItemCountAsync(string? partition = null, CancellationToken cancellationToken = default)
    {
        QueryDefinition queryDefinition = new("SELECT COUNT(1) FROM c");
        var enumerable = await ExecuteQueryAsync<CountResponse>(queryDefinition, partition, cancellationToken);
        return enumerable.Count;
    }

    /// <summary>
    /// Read all items across all partitions. This is an expensive operation and should be avoided if possible.
    /// </summary>
    protected async Task<IEnumerable<TEntity>> ReadAllItemsAsync(CancellationToken cancellationToken)
    {
        QueryDefinition queryDefinition = new("SELECT * FROM c");
        return await ExecuteQueryAsync<TEntity>(queryDefinition, cancellationToken: cancellationToken);
    }

    protected async Task<IReadOnlyCollection<TResponse>> ExecuteQueryAsync<TResponse>(
        QueryDefinition queryDefinition,
        string? partition = null,
        CancellationToken cancellationToken = default)
    {
        using var iterator = container.GetItemQueryIterator<TResponse>(queryDefinition,
            requestOptions: new QueryRequestOptions
            {
                PartitionKey = partition is not null ? new PartitionKey(partition) : null
            });

        logger.LogInformation("Executing SQL query: {QueryText}, on partition: {Partition}", queryDefinition.QueryText,
            partition);

        List<TResponse> items = new();
        double totalCharge = 0;
        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync(cancellationToken);
            logger.LogInformation("Read {Count} items, charge was: {Charge} RU", response.Count,
                response.RequestCharge);
            totalCharge += response.RequestCharge;
            items.AddRange(response);
        }

        logger.LogInformation("Completed query. Total charge: {Total} RU", totalCharge);

        return items;
    }
}
