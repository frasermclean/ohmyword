using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OhMyWord.Services.Options;
using System.Net;

namespace OhMyWord.Services.Data;

public interface ICosmosDbService
{
    Task<Container> GetContainerAsync(ContainerId containerId, CancellationToken cancellationToken = default);
}

public class CosmosDbService : ICosmosDbService
{
    private readonly ILogger<CosmosDbService> logger;
    private readonly CosmosClient cosmosClient;
    private Database? database;

    private string DatabaseId { get; }

    private readonly Dictionary<ContainerId, string> containerDetails = new()
    {
        { ContainerId.Words, "/partOfSpeech" },
        { ContainerId.Players, "/id" }
    };

    public CosmosDbService(IOptions<CosmosDbOptions> options, ILogger<CosmosDbService> logger, IHttpClientFactory httpClientFactory)
    {
        this.logger = logger;
        logger.LogInformation("Creating Cosmos DB client.");

        cosmosClient = new CosmosClient(options.Value.Endpoint, options.Value.PrimaryKey, new CosmosClientOptions()
        {
            HttpClientFactory = httpClientFactory.CreateClient,
            Serializer = new EntitySerializer()
        });

        DatabaseId = options.Value.DatabaseId;
    }

    private async Task<Database> GetDatabaseAsync(CancellationToken cancellationToken = default)
    {
        var response = await cosmosClient.CreateDatabaseIfNotExistsAsync(DatabaseId, cancellationToken: cancellationToken);

        switch (response.StatusCode)
        {
            case HttpStatusCode.Created:
                logger.LogInformation("Created new database with ID: {databaseId}", DatabaseId);
                break;
            case HttpStatusCode.OK:
                logger.LogInformation("Connected to existing database with ID: {databaseId}", DatabaseId);
                break;
        }

        return response.Database;
    }

    public async Task<Container> GetContainerAsync(ContainerId containerId, CancellationToken cancellationToken = default)
    {
        database ??= await GetDatabaseAsync(cancellationToken);

        var partitionKeyPath = containerDetails[containerId];
        var response = await database.CreateContainerIfNotExistsAsync(containerId.ToString(), partitionKeyPath, cancellationToken: cancellationToken);

        switch (response.StatusCode)
        {
            case HttpStatusCode.Created:
                logger.LogInformation("Created new container with ID: {containerId} and partition key path: {partitionKeyPath}", containerId, partitionKeyPath);
                break;
            case HttpStatusCode.OK:
                logger.LogInformation("Connected to existing container with ID: {containerId}", containerId);
                break;
        }

        return response.Container;
    }
}
