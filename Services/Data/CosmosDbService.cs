using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;
using Microsoft.Extensions.Options;
using OhMyWord.Services.Options;

namespace OhMyWord.Services.Data;

public interface ICosmosDbService
{
    Container GetContainer(string containerId);
}

public class CosmosDbService : ICosmosDbService, IDisposable
{
    private readonly CosmosClient cosmosClient;
    private readonly Database database;

    public CosmosDbService(IOptions<CosmosDbOptions> options, IHttpClientFactory httpClientFactory)
    {
        cosmosClient = new CosmosClientBuilder(options.Value.Endpoint, options.Value.PrimaryKey)
            .WithApplicationName("OhMyWord API")
            .WithHttpClientFactory(httpClientFactory.CreateClient)
            .WithCustomSerializer(new EntitySerializer())
            .Build();

        database = cosmosClient.GetDatabase(options.Value.DatabaseId);
    }

    public Container GetContainer(string containerId) => database.GetContainer(containerId);

    public void Dispose()
    {
        cosmosClient.Dispose();
        GC.SuppressFinalize(this);
    }
}
