using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using OhMyWord.Core.Models;

namespace OhMyWord.Services.Data.Repositories;

public abstract class Repository<TEntity> where TEntity : Entity
{
    private readonly ILogger<Repository<TEntity>> logger;
    private readonly Container container;

    protected Repository(ICosmosDbService cosmosDbService, ILogger<Repository<TEntity>> logger, string containerId)
    {
        this.logger = logger;
        container = cosmosDbService.GetContainer(containerId);
    }

    protected async Task<RepositoryActionResult<TEntity>> CreateItemAsync(TEntity item, CancellationToken cancellationToken = default)
    {
        var partitionKey = new PartitionKey(item.GetPartition());
        await using var stream = EntitySerializer.ConvertToStream(item);
        var response = await container.CreateItemStreamAsync(stream, partitionKey, cancellationToken: cancellationToken);
        LogResponseMessage(response, RepositoryAction.Create, item.Id, item.GetPartition());
        return new RepositoryActionResult<TEntity>(response, RepositoryAction.Create, item.Id);
    }

    protected async Task<RepositoryActionResult<TEntity>> ReadItemAsync(string id, string partition, CancellationToken cancellationToken = default)
    {
        var partitionKey = new PartitionKey(partition);
        using var response = await container.ReadItemStreamAsync(id, partitionKey, cancellationToken: cancellationToken);
        LogResponseMessage(response, RepositoryAction.Read, id, partition);
        return new RepositoryActionResult<TEntity>(response, RepositoryAction.Read, id);
    }

    protected async Task<RepositoryActionResult<TEntity>> UpdateItemAsync(TEntity item, CancellationToken cancellationToken = default)
    {
        var partitionKey = new PartitionKey(item.GetPartition());
        await using var stream = EntitySerializer.ConvertToStream(item);
        var response = await container.ReplaceItemStreamAsync(stream, item.Id, partitionKey, cancellationToken: cancellationToken);
        LogResponseMessage(response, RepositoryAction.Update, item.Id, item.GetPartition());
        return new RepositoryActionResult<TEntity>(response, RepositoryAction.Update, item.Id);
    }

    protected Task DeleteItemAsync(TEntity item) => DeleteItemAsync(item.Id, item.GetPartition());

    protected async Task<RepositoryActionResult<TEntity>> DeleteItemAsync(string id, string partition, CancellationToken cancellationToken = default)
    {
        var partitionKey = new PartitionKey(partition);
        var response = await container.DeleteItemStreamAsync(id, partitionKey, cancellationToken: cancellationToken);
        LogResponseMessage(response, RepositoryAction.Delete, id, partition);
        return new RepositoryActionResult<TEntity>(response, RepositoryAction.Delete, id);
    }

    protected async Task<RepositoryActionResult<TEntity>> PatchItemAsync(string id, string partition, PatchOperation[] operations, CancellationToken cancellationToken = default)
    {
        var partitionKey = new PartitionKey(partition);
        var response = await container.PatchItemStreamAsync(id, partitionKey, operations, cancellationToken: cancellationToken);
        LogResponseMessage(response, RepositoryAction.Patch, id, partition);
        return new RepositoryActionResult<TEntity>(response, RepositoryAction.Patch, id);
    }

    #region Multiple item enumeration methods

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

    private void LogResponseMessage(ResponseMessage response, RepositoryAction action, string id, string partition)
    {
        var entityTypeName = typeof(TEntity).Name.ToLowerInvariant();

        if (!response.IsSuccessStatusCode)
        {
            logger.LogError("Could not {action} {typeName} with ID: {id} on partition: {partition}.",
                action.ToString().ToLowerInvariant(), entityTypeName, id, partition);
            return;
        }

        var actionPastTense = action switch
        {
            RepositoryAction.Create => "Created",
            RepositoryAction.Update => "Updated",
            RepositoryAction.Delete => "Deleted",
            RepositoryAction.Patch => "Patched",
            _ => action.ToString()
        };

        logger.LogInformation("{ActionPastTense} {typeName} with ID: {id} on partition: {partition}.",
            actionPastTense, entityTypeName, id, partition);
    }
}
