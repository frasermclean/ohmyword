using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;
using Microsoft.Extensions.Options;
using OhMyWord.Data.Options;

namespace OhMyWord.Data.Services;

public interface ICosmosDbService
{
    Task<Container> GetContainerAsync(string containerId, string partitionKeyPath);
}

public class CosmosDbService : ICosmosDbService, IDisposable
{
    private readonly CosmosClient cosmosClient;
    private readonly Task<DatabaseResponse> databaseResponseTask;

    public CosmosDbService(IOptions<CosmosDbOptions> options, IHttpClientFactory httpClientFactory)
    {
        cosmosClient = new CosmosClientBuilder(options.Value.Endpoint, options.Value.PrimaryKey)
            .WithApplicationName("OhMyWord API")
            .WithHttpClientFactory(httpClientFactory.CreateClient)
            .WithCustomSerializer(new EntitySerializer())
            .Build();

        databaseResponseTask = cosmosClient.CreateDatabaseIfNotExistsAsync(options.Value.DatabaseId);
    }

    public async Task<Container> GetContainerAsync(string containerId, string partitionKeyPath)
    {
        var databaseResponse = await databaseResponseTask;

        return await databaseResponse.Database
            .CreateContainerIfNotExistsAsync(containerId, partitionKeyPath)
            .ConfigureAwait(false);
    }

    public void Dispose()
    {
        cosmosClient.Dispose();
        GC.SuppressFinalize(this);
    }
}
