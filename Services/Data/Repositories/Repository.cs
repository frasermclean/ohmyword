using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using OhMyWord.Core.Models;

namespace OhMyWord.Services.Data.Repositories;

public abstract class Repository<TEntity> where TEntity : Entity
{
    private readonly ICosmosDbService cosmosDbService;
    private readonly ILogger<Repository<TEntity>> logger;

    private ContainerId ContainerId { get; }
    private string EntityTypeName { get; }

    protected Repository(ICosmosDbService cosmosDbService, ILogger<Repository<TEntity>> logger, ContainerId containerId)
    {
        this.cosmosDbService = cosmosDbService;
        this.logger = logger;

        ContainerId = containerId;
        EntityTypeName = typeof(TEntity).Name;
    }

    private Task<Container> GetContainerAsync(CancellationToken cancellation = default) =>
        cosmosDbService.GetContainerAsync(ContainerId, cancellation);

    protected async Task<TEntity> CreateItemAsync(TEntity item)
    {
        var container = await GetContainerAsync();
        var response = await container.CreateItemAsync(item);

        logger.LogDebug("Created {typeName} with ID: {id}. Resource charge: {charge} RU.", EntityTypeName, item.Id, response.RequestCharge);

        return response.Resource;
    }

    protected async Task<TEntity?> ReadItemAsync(string id, string partition)
    {
        var container = await GetContainerAsync();
        var partitionKey = new PartitionKey(partition);

        using var response = await container.ReadItemStreamAsync(id, partitionKey);

        return response.IsSuccessStatusCode ? EntitySerializer.ConvertFromStream<TEntity>(response.Content) : null;
    }

    protected async Task<RepositoryActionResult<TEntity>> UpdateItemAsync(TEntity item, string id, string partition)
    {
        var container = await GetContainerAsync();
        var partitionKey = new PartitionKey(partition);

        await using var stream = EntitySerializer.ConvertToStream(item);
        var response = await container.ReplaceItemStreamAsync(stream, id, partitionKey);

        return new RepositoryActionResult<TEntity>
        {
            Success = response.IsSuccessStatusCode,
            StatusCode = response.StatusCode,
            ErrorMessage = response.ErrorMessage,
            Resource = item,
        };
    }

    protected Task DeleteItemAsync(TEntity item) => DeleteItemAsync(item.Id, item.GetPartition());

    protected async Task<bool> DeleteItemAsync(string id, string partition)
    {
        var container = await GetContainerAsync();
        var partitionKey = new PartitionKey(partition);
        var response = await container.DeleteItemStreamAsync(id, partitionKey);

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
        string? partition = null)
    {
        var container = await GetContainerAsync();

        using var iterator = container.GetItemQueryIterator<TResponse>(queryDefinition, requestOptions: new QueryRequestOptions
        {
            PartitionKey = partition is not null ? new PartitionKey(partition) : null
        });

        logger.LogInformation("Executing SQL query: {queryText}, on partition: {partition}.", queryDefinition.QueryText, partition);

        List<TResponse> items = new();
        double totalCharge = 0;

        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync();
            logger.LogInformation("Read {count} items, charge was: {charge} RU.", response.Count, response.RequestCharge);
            totalCharge += response.RequestCharge;
            items.AddRange(response);
        }

        logger.LogInformation("Completed query. Total charge: {total} RU.", totalCharge);

        return items;
    }

    #endregion
}
