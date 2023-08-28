using FluentResults;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using OhMyWord.Data.CosmosDb.Errors;
using OhMyWord.Data.CosmosDb.Models;
using System.Net;

namespace OhMyWord.Data.CosmosDb.Services;

public interface IContainerManager
{
    Task<Result<T>> CreateItemAsync<T>(T item, CancellationToken cancellationToken = default)
        where T : Item;

    Task<Result<T>> ReadItemAsync<T>(string id, string partition, CancellationToken cancellationToken = default)
        where T : Item;
}

public class ContainerManager : IContainerManager
{
    private readonly Container container;
    private readonly ILogger<ContainerManager> logger;

    public ContainerManager(Container container, ILogger<ContainerManager> logger)
    {
        this.container = container;
        this.logger = logger;
    }

    public async Task<Result<T>> CreateItemAsync<T>(T item, CancellationToken cancellationToken = default)
        where T : Item
    {
        var partition = item.GetPartition();

        try
        {
            var response = await container.CreateItemAsync(item, new PartitionKey(partition),
                cancellationToken: cancellationToken);

            logger.LogInformation("Created item on partition: /{Partition}, request charge: {RequestCharge}",
                partition, response.RequestCharge);

            return item;
        }
        catch (CosmosException exception) when (exception.StatusCode == HttpStatusCode.Conflict)
        {
            logger.LogError(exception, "Conflict creating item with ID: {Id} on partition: /{Partition}",
                item.Id, partition);

            return new ItemConflictError(item.Id, partition).CausedBy(exception);
        }
    }

    public async Task<Result<T>> ReadItemAsync<T>(string id, string partition,
        CancellationToken cancellationToken = default) where T : Item
    {
        try
        {
            var response = await container.ReadItemAsync<T>(id, new PartitionKey(partition),
                cancellationToken: cancellationToken);

            logger.LogInformation("Read item on partition: /{Partition}, request charge: {RequestCharge}",
                partition, response.RequestCharge);

            return response.Resource;
        }
        catch (CosmosException exception) when (exception.StatusCode == HttpStatusCode.NotFound)
        {
            logger.LogWarning(exception, "Item not found: {Id} on partition: /{Partition}", id, partition);
            return new ItemNotFoundError(id, partition).CausedBy(exception);
        }
    }
}
