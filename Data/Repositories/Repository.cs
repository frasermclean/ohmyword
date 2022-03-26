using Humanizer;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using OhMyWord.Data.Models;

namespace OhMyWord.Data.Repositories;

public abstract class Repository<TEntity> where TEntity : Entity
{
    private readonly Task<Container> containerTask;
    private readonly ILogger<Repository<TEntity>> logger;

    private string EntityTypeName { get; }

    protected Repository(ICosmosDbService cosmosDbService, ILogger<Repository<TEntity>> logger, string? containerId = null, string partitionKeyPath = "/id")
    {
        EntityTypeName = typeof(TEntity).Name;
        containerId ??= EntityTypeName.Pluralize();
        containerTask = cosmosDbService.GetContainerAsync(containerId, partitionKeyPath);
        this.logger = logger;
    }

    private Task<Container> GetContainerAsync() => containerTask;

    protected async Task<TEntity> CreateItemAsync(TEntity item)
    {
        var container = await GetContainerAsync();
        var response = await container.CreateItemAsync(item);

        logger.LogDebug("Created {typeName} with ID: {id}. Resource charge: {charge} RU.", EntityTypeName, item.Id, response.RequestCharge);

        return response.Resource;
    }

    protected async Task DeleteItemAsync(string id, string? partition = null)
    {
        var container = await GetContainerAsync();
        var partitionKey = new PartitionKey(partition ?? id);
        var response = await container.DeleteItemAsync<TEntity>(id, partitionKey);

        logger.LogDebug("Deleted {typeName} with ID: {id} on partition: {partition}. Resource charge: {charge} RU.", EntityTypeName, id, partitionKey, response.RequestCharge);
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
