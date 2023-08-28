using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OhMyWord.Data.CosmosDb.Options;

namespace OhMyWord.Data.CosmosDb.Services;

public sealed class ContainerManagerFactory
{
    private readonly CosmosClient cosmosClient;
    private readonly IServiceProvider serviceProvider;
    private readonly string databaseId;

    public ContainerManagerFactory(CosmosClient cosmosClient, IOptions<CosmosDbOptions> options,
        IServiceProvider serviceProvider)
    {
        this.cosmosClient = cosmosClient;
        this.serviceProvider = serviceProvider;
        databaseId = options.Value.DatabaseId;
    }

    /// <summary>
    /// Create a new container manager for the specified containerId.
    /// </summary>
    /// <param name="containerId">The container ID to create a manager for.</param>
    /// <returns>A <see cref="IContainerManager"/> instance.</returns>
    public IContainerManager Create(string containerId)
    {
        var container = cosmosClient.GetContainer(databaseId, containerId);
        var logger = serviceProvider.GetRequiredService<ILogger<ContainerManager>>();
        return new ContainerManager(container, logger);
    }
}
