using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;
using OhMyWord.Infrastructure;
using ContainerBuilder = DotNet.Testcontainers.Builders.ContainerBuilder;

namespace Infrastructure.Tests.Fixtures;

public sealed class CosmosDbEmulatorFixture : IDisposable, IAsyncLifetime
{
    private readonly IContainer container;
    private readonly Lazy<CosmosClient> cosmosClient;

    private const string DatabaseId = "test-db";
    private const string IpAddress = "127.0.0.1";
    private const int PartitionCount = 5;

    public CosmosDbEmulatorFixture()
    {
        container = new ContainerBuilder()
            .WithImage("mcr.microsoft.com/cosmosdb/linux/azure-cosmos-emulator")
            .WithName("azure-cosmos-emulator")
            .WithExposedPort(8081)
            .WithExposedPort(10251)
            .WithExposedPort(10252)
            .WithExposedPort(10253)
            .WithExposedPort(10254)
            .WithPortBinding(8081, 8081)
            .WithPortBinding(10251, true)
            .WithPortBinding(10252, true)
            .WithPortBinding(10253, true)
            .WithPortBinding(10254, true)
            .WithEnvironment("AZURE_COSMOS_EMULATOR_PARTITION_COUNT", PartitionCount.ToString())
            .WithEnvironment("AZURE_COSMOS_EMULATOR_IP_ADDRESS_OVERRIDE", IpAddress)
            .WithOutputConsumer(Consume.RedirectStdoutAndStderrToStream(new MemoryStream(), new MemoryStream()))
            .WithWaitStrategy(Wait.ForUnixContainer()
                .UntilMessageIsLogged("Started"))
            .Build();

        cosmosClient = new Lazy<CosmosClient>(() =>
        {
            var port = container.GetMappedPublicPort(8081);
            var endpoint = $"https://{IpAddress}:{port}";
            const string authKey =
                "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";

            return new CosmosClientBuilder(endpoint, authKey)
                .WithHttpClientFactory(() => new HttpClient(new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback =
                        HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                }))
                .WithCustomSerializer(new EntitySerializer())
                .WithConnectionModeGateway()
                .Build();
        });
    }

    public CosmosClient CosmosClient => cosmosClient.Value;

    public async Task InitializeAsync()
    {
        // start the test container
        await container.StartAsync();

        // create database
        var database = await CosmosClient.CreateDatabaseIfNotExistsAsync(DatabaseId);

        // create containers
        await database.Database.CreateContainerIfNotExistsAsync("words", "/id");
    }

    public void Dispose()
    {
        if (cosmosClient.IsValueCreated)
        {
            cosmosClient.Value.Dispose();
        }
    }

    public Task DisposeAsync() => container.DisposeAsync().AsTask();
}
