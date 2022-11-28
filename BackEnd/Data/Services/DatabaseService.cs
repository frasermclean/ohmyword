using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;
using Microsoft.Extensions.Options;
using OhMyWord.Data.Options;

namespace OhMyWord.Data.Services;

public interface IDatabaseService
{
    Container GetContainer(string containerId);
}

public sealed class DatabaseService : IDatabaseService, IDisposable
{
    private readonly CosmosClient cosmosClient;
    private readonly Database database;

    public DatabaseService(IOptions<CosmosDbOptions> options, IHttpClientFactory httpClientFactory)
    {
        cosmosClient = new CosmosClientBuilder(options.Value.ConnectionString)
            .WithApplicationName(options.Value.ApplicationName)
            .WithHttpClientFactory(() => httpClientFactory.CreateClient("CosmosDb"))            
            .WithCustomSerializer(new EntitySerializer())
            .Build();

        database = cosmosClient.GetDatabase(options.Value.DatabaseId);
    }

    public Container GetContainer(string containerId) => database.GetContainer(containerId);

    public void Dispose()
    {
        cosmosClient.Dispose();        
    }
}
