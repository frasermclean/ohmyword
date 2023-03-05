using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OhMyWord.Data.Entities;
using OhMyWord.Data.Options;
using System.Runtime.CompilerServices;
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
        IgnoreReadOnlyProperties = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
    };

    protected Repository(CosmosClient cosmosClient, IOptions<CosmosDbOptions> options,
        ILogger<Repository<TEntity>> logger, string containerId)
    {
        container = cosmosClient.GetContainer(options.Value.DatabaseId, containerId);
        this.logger = logger;
        entityTypeName = typeof(TEntity).Name;
    }

    protected async Task CreateItemAsync(TEntity item, CancellationToken cancellationToken = default)
    {
        using var stream = new MemoryStream();
        await JsonSerializer.SerializeAsync(stream, item, serializerOptions, cancellationToken);
        var response = await container.CreateItemStreamAsync(stream, new PartitionKey(item.GetPartition()),
            cancellationToken: cancellationToken);

        response.EnsureSuccessStatusCode();

        logger.LogInformation("Created {TypeName} on partition: /{Partition}", entityTypeName, item.GetPartition());
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

    protected async Task UpdateItemAsync(TEntity item,
        CancellationToken cancellationToken = default)
    {
        using var stream = new MemoryStream();
        await JsonSerializer.SerializeAsync(stream, item, serializerOptions, cancellationToken);
        var response = await container.ReplaceItemStreamAsync(stream, item.Id, new PartitionKey(item.GetPartition()),
            cancellationToken: cancellationToken);

        response.EnsureSuccessStatusCode();

        logger.LogInformation("Replaced {TypeName} on partition: /{Partition}", entityTypeName, item.GetPartition());
    }

    protected Task DeleteItemAsync(TEntity item) => DeleteItemAsync(item.Id, item.GetPartition());

    protected async Task DeleteItemAsync(string id, string partition, CancellationToken cancellationToken = default)
    {
        var response = await container.DeleteItemStreamAsync(id, new PartitionKey(partition),
            cancellationToken: cancellationToken);

        response.EnsureSuccessStatusCode();

        logger.LogInformation("Deleted item: {Id} on partition: /{Partition}", id, partition);
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
        var response =
            await ExecuteQuery<CountResponse>(queryDefinition, partition, cancellationToken: cancellationToken)
                .FirstOrDefaultAsync(cancellationToken);

        return response?.Count ?? 0;
    }

    /// <summary>
    /// Read all items on the specified partition.
    /// </summary>
    /// <param name="partition">The partition to query items on. If null, will query all partitions</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns></returns>
    protected IAsyncEnumerable<TEntity> ReadPartitionItems(string? partition = null,
        CancellationToken cancellationToken = default) =>
        ExecuteQuery<TEntity>(new QueryDefinition("SELECT * FROM c"), partition, cancellationToken: cancellationToken);

    protected IAsyncEnumerable<string> ReadItemIds(string? partition = null,
        CancellationToken cancellationToken = default) =>
        ExecuteQuery<string>(new QueryDefinition("SELECT * FROM c.id"), partition,
            cancellationToken: cancellationToken);

    protected async IAsyncEnumerable<TResponse> ExecuteQuery<TResponse>(QueryDefinition queryDefinition,
        string? partition = null, int maxItemCount = -1,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        using var iterator = container.GetItemQueryIterator<TResponse>(queryDefinition,
            requestOptions: new QueryRequestOptions
            {
                MaxItemCount = maxItemCount,
                PartitionKey = partition is not null ? new PartitionKey(partition) : null,
            });

        logger.LogInformation("Executing SQL query: {QueryText}, on partition: {Partition}", queryDefinition.QueryText,
            partition);

        int total = 0;
        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync(cancellationToken);
            total += response.Count;
            foreach (var item in response)
                yield return item;
        }

        logger.LogInformation("Executed SQL query: {QueryText}, on partition: {Partition}, total results: {Total}",
            queryDefinition.QueryText, partition, total);
    }
}
