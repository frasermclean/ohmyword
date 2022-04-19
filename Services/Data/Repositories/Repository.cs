using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using OhMyWord.Core.Models;

namespace OhMyWord.Services.Data.Repositories;

public abstract class Repository<TEntity> where TEntity : Entity
{
    private readonly ICosmosDbService cosmosDbService;
    private readonly ILogger<Repository<TEntity>> logger;

    private ContainerId ContainerId { get; }
    private Container? container;
    private string EntityTypeName { get; }

    protected Repository(ICosmosDbService cosmosDbService, ILogger<Repository<TEntity>> logger, ContainerId containerId)
    {
        this.cosmosDbService = cosmosDbService;
        this.logger = logger;

        ContainerId = containerId;
        EntityTypeName = typeof(TEntity).Name;
    }

    private Task<Container> GetContainerAsync(CancellationToken cancellationToken = default) =>
        cosmosDbService.GetContainerAsync(ContainerId, cancellationToken);

    protected async Task<TEntity> CreateItemAsync(TEntity item, CancellationToken cancellationToken = default)
    {
        container ??= await GetContainerAsync(cancellationToken);
        var response = await container.CreateItemAsync(item, cancellationToken: cancellationToken);

        logger.LogDebug("Created {typeName} with ID: {id}. Resource charge: {charge} RU.", EntityTypeName, item.Id, response.RequestCharge);

        return response.Resource;
    }

    protected async Task<RepositoryActionResult<TEntity>> ReadItemAsync(string id, string partition, CancellationToken cancellationToken = default)
    {
        container ??= await GetContainerAsync(cancellationToken);
        var partitionKey = new PartitionKey(partition);

        using var response = await container.ReadItemStreamAsync(id, partitionKey, cancellationToken: cancellationToken);

        return RepositoryActionResult<TEntity>.FromResponseMessage(response, RepositoryAction.Read, id);
    }

    protected async Task<RepositoryActionResult<TEntity>> UpdateItemAsync(TEntity item, string id, string partition, CancellationToken cancellationToken = default)
    {
        container ??= await GetContainerAsync(cancellationToken);
        var partitionKey = new PartitionKey(partition);

        await using var stream = EntitySerializer.ConvertToStream(item);
        var response = await container.ReplaceItemStreamAsync(stream, id, partitionKey, cancellationToken: cancellationToken);

        return RepositoryActionResult<TEntity>.FromResponseMessage(response, RepositoryAction.Update, id);
    }

    protected Task DeleteItemAsync(TEntity item) => DeleteItemAsync(item.Id, item.GetPartition());

    protected async Task<bool> DeleteItemAsync(string id, string partition, CancellationToken cancellationToken = default)
    {
        container ??= await GetContainerAsync(cancellationToken);
        var partitionKey = new PartitionKey(partition);
        var response = await container.DeleteItemStreamAsync(id, partitionKey, cancellationToken: cancellationToken);

        if (!response.IsSuccessStatusCode) return false;

        logger.LogDebug("Deleted {typeName} with ID: {id} on partition: {partition}.", EntityTypeName, id, partitionKey);
        return true;
    }

    #region Multiple item enumeration methods

    /// <summary>
    /// Read all items across all partitions. This is an expensive operation and should be avoided if possible.
    /// </summary>
    protected async Task<IEnumerable<TEntity>> ReadAllItemsAsync()
    {
        QueryDefinition queryDefinition = new("SELECT * FROM c");
        return await ExecuteQueryAsync<TEntity>(queryDefinition);
    }

    protected async Task<IEnumerable<TResponse>> ExecuteQueryAsync<TResponse>(
        QueryDefinition queryDefinition,
        string? partition = null,
        CancellationToken cancellationToken = default)
    {
        container ??= await GetContainerAsync(cancellationToken);

        using var iterator = container.GetItemQueryIterator<TResponse>(queryDefinition, requestOptions: new QueryRequestOptions
        {
            PartitionKey = partition is not null ? new PartitionKey(partition) : null
        });

        logger.LogInformation("Executing SQL query: {queryText}, on partition: {partition}.", queryDefinition.QueryText, partition);

        List<TResponse> items = new();
        double totalCharge = 0;

        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync(cancellationToken);
            logger.LogInformation("Read {count} items, charge was: {charge} RU.", response.Count, response.RequestCharge);
            totalCharge += response.RequestCharge;
            items.AddRange(response);
        }

        logger.LogInformation("Completed query. Total charge: {total} RU.", totalCharge);

        return items;
    }

    #endregion
}
