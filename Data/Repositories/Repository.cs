using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using OhMyWord.Data.Models;

namespace OhMyWord.Data.Repositories;

public abstract class Repository<TEntity> where TEntity : Entity
{
    private readonly Task<Container> containerTask;
    private readonly ILogger<Repository<TEntity>> logger;

    protected abstract string ContainerId { get; }
    protected abstract string PartitionKeyPath { get; }

    public Repository(ICosmosDbService cosmosDbService, ILogger<Repository<TEntity>> logger)
    {
        containerTask = cosmosDbService.GetContainerAsync(ContainerId, PartitionKeyPath);
        this.logger = logger;
    }

    protected async Task<Container> GetContainerAsync() => await containerTask;

    #region Multiple item enumeration methods

    /// <summary>
    /// Read all items across all partitions. This is an expensive operation and should be avoided if possible.
    /// </summary>
    /// <returns>An enumerable of <typeparamref name="T"/></returns>
    protected async Task<IEnumerable<TEntity>> ReadAllItemsAsync()
    {
        QueryDefinition queryDefinition = new("SELECT * FROM c");
        return await ExecuteQueryAsync<TEntity>(queryDefinition);
    }

    private async Task<IEnumerable<TResponse>> ExecuteQueryAsync<TResponse>(
        QueryDefinition queryDefinition,
        string partition = default!)
    {
        Container container = await GetContainerAsync();

        using FeedIterator<TResponse> iterator = container.GetItemQueryIterator<TResponse>(queryDefinition, requestOptions: new()
        {
            PartitionKey = partition is not null ? new PartitionKey(partition) : null
        });

        logger.LogInformation("Executing SQL query: {queryText}, on partition: {partition}.", queryDefinition.QueryText, partition);

        List<TResponse> items = new();
        double totalCharge = 0;

        while (iterator.HasMoreResults)
        {
            FeedResponse<TResponse> response = await iterator.ReadNextAsync();
            logger.LogInformation("Read {count} items, charge was: {charge} RU.", response.Count, response.RequestCharge);
            totalCharge += response.RequestCharge;
            items.AddRange(response);
        }

        logger.LogInformation("Completed query. Total charge: {total} RU.", totalCharge);

        return items;
    }

    #endregion
}
