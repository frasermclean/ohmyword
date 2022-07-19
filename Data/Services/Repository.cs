using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using OhMyWord.Data.Models;

namespace OhMyWord.Data.Services;

public abstract class Repository<TEntity> where TEntity : Entity
{
    private readonly ILogger<Repository<TEntity>> logger;
    private readonly Task<Container> containerTask;
    private readonly string entityTypeName;

    protected Repository(ICosmosDbService cosmosDbService, ILogger<Repository<TEntity>> logger, string containerId,
        string partitionKeyPath = "/id")
    {
        this.logger = logger;
        containerTask = cosmosDbService.GetContainerAsync(containerId, partitionKeyPath);
        entityTypeName = typeof(TEntity).Name;
    }

    protected async Task<TEntity> CreateItemAsync(TEntity item, CancellationToken cancellationToken = default)
    {
        var container = await GetContainerAsync();
        var response = await container.CreateItemAsync(item, new PartitionKey(item.GetPartition()),
            cancellationToken: cancellationToken);

        logger.LogInformation("Created {typeName} on partition: /{partition}, request charge: {charge} RU",
            entityTypeName, item.GetPartition(), response.RequestCharge);

        return response.Resource;
    }

    protected async Task<TEntity?> ReadItemAsync(Guid id, string partition,
        CancellationToken cancellationToken = default)
    {
        var container = await GetContainerAsync();
        var response = await container.ReadItemAsync<TEntity>(id.ToString(), new PartitionKey(partition),
            cancellationToken: cancellationToken);

        logger.LogInformation("Read {typeName} on partition: /{partition}, request charge: {charge} RU",
            entityTypeName, partition, response.RequestCharge);

        return response.Resource;
    }

    protected async Task<TEntity> UpdateItemAsync(TEntity item,
        CancellationToken cancellationToken = default)
    {
        var container = await GetContainerAsync();
        var response = await container.ReplaceItemAsync(item, item.Id.ToString(), new PartitionKey(item.GetPartition()),
            cancellationToken: cancellationToken);

        logger.LogInformation("Replaced {typeName} on partition: /{partition}, request charge: {charge} RU",
            entityTypeName, item.GetPartition(), response.RequestCharge);

        return response.Resource;
    }

    protected Task DeleteItemAsync(TEntity item) => DeleteItemAsync(item.Id, item.GetPartition());

    protected async Task DeleteItemAsync(Guid id, string partition,
        CancellationToken cancellationToken = default)
    {
        var container = await GetContainerAsync();
        var response = await container.DeleteItemAsync<TEntity>(id.ToString(), new PartitionKey(partition),
            cancellationToken: cancellationToken);

        logger.LogInformation("Deleted {typeName} on partition: /{partition}, request charge: {charge} RU",
            entityTypeName, partition, response.RequestCharge);
    }

    protected async Task<TEntity> PatchItemAsync(
        Guid id,
        string partition,
        PatchOperation[] operations,
        CancellationToken cancellationToken = default)
    {
        var container = await GetContainerAsync();
        var response = await container.PatchItemAsync<TEntity>(id.ToString(), new PartitionKey(partition), operations,
            cancellationToken: cancellationToken);

        logger.LogInformation("Patched {typeName} on partition: /{partition}, request charge: {charge} RU",
            entityTypeName, partition, response.RequestCharge);

        return response.Resource;
    }

    /// <summary>
    /// Read all items across all partitions. This is an expensive operation and should be avoided if possible.
    /// </summary>
    protected async Task<IEnumerable<TEntity>> ReadAllItemsAsync(CancellationToken cancellationToken)
    {
        QueryDefinition queryDefinition = new("SELECT * FROM c");
        return await ExecuteQueryAsync<TEntity>(queryDefinition, cancellationToken: cancellationToken);
    }

    protected async Task<IEnumerable<TResponse>> ExecuteQueryAsync<TResponse>(
        QueryDefinition queryDefinition,
        string? partition = null,
        CancellationToken cancellationToken = default)
    {
        var container = await GetContainerAsync();

        using var iterator = container.GetItemQueryIterator<TResponse>(queryDefinition,
            requestOptions: new QueryRequestOptions
            {
                PartitionKey = partition is not null ? new PartitionKey(partition) : null
            });

        logger.LogInformation("Executing SQL query: {queryText}, on partition: {partition}.", queryDefinition.QueryText,
            partition);

        List<TResponse> items = new();
        double totalCharge = 0;

        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync(cancellationToken);
            logger.LogInformation("Read {count} items, charge was: {charge} RU.", response.Count,
                response.RequestCharge);
            totalCharge += response.RequestCharge;
            items.AddRange(response);
        }

        logger.LogInformation("Completed query. Total charge: {total} RU.", totalCharge);

        return items;
    }

    private async Task<Container> GetContainerAsync() => await containerTask.ConfigureAwait(false);
}