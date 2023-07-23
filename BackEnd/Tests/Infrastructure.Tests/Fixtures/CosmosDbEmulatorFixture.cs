using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;
using Microsoft.Extensions.Options;
using OhMyWord.Infrastructure;
using OhMyWord.Infrastructure.Options;
using ContainerBuilder = DotNet.Testcontainers.Builders.ContainerBuilder;

namespace Infrastructure.Tests.Fixtures;

public sealed class CosmosDbEmulatorFixture : IDisposable, IAsyncLifetime
{
    private readonly IContainer container;
    private readonly Lazy<CosmosClient> cosmosClient;

    private const string DatabaseId = "test-db";
    private const string IpAddress = "127.0.0.1";
    private const int ContainerPort = 8081;
    private const int PartitionCount = 5;

    public CosmosDbEmulatorFixture()
    {
        container = new ContainerBuilder()
            .WithImage("mcr.microsoft.com/cosmosdb/linux/azure-cosmos-emulator")
            .WithPortBinding(ContainerPort)
            .WithEnvironment("AZURE_COSMOS_EMULATOR_PARTITION_COUNT", PartitionCount.ToString())
            .WithEnvironment("AZURE_COSMOS_EMULATOR_IP_ADDRESS_OVERRIDE", IpAddress)
            .WithWaitStrategy(Wait.ForUnixContainer()
                .UntilPortIsAvailable(ContainerPort)
                .UntilMessageIsLogged($"Started {PartitionCount + 1}/{PartitionCount + 1} partitions"))
            .Build();

        cosmosClient = new Lazy<CosmosClient>(() =>
        {
            var endpoint = $"https://{container.Hostname}:{container.GetMappedPublicPort(ContainerPort)}/";
            const string authKey =
                "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";

            return new CosmosClientBuilder(endpoint, authKey)
                .WithHttpClientFactory(() => new HttpClient(new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback =
                        HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                }))
                .WithCustomSerializer(EntitySerializer.Instance)
                .WithConnectionModeGateway()
                .Build();
        });
    }

    public CosmosClient CosmosClient => cosmosClient.Value;

    public IOptions<CosmosDbOptions> Options =>
        Microsoft.Extensions.Options.Options.Create(new CosmosDbOptions { DatabaseId = DatabaseId, });

    public async Task InitializeAsync()
    {
        // start the test container
        await container.StartAsync();

        // create database
        var database = await cosmosClient.Value.CreateDatabaseAsync(DatabaseId);

        // create containers
        await database.Database.CreateContainerAsync("words", "/id");
        await database.Database.CreateContainerAsync("definitions", "/wordId");
    }

    public void Dispose()
    {
        if (cosmosClient.IsValueCreated)
        {
            cosmosClient.Value.Dispose();
        }
    }

    public async Task DisposeAsync()
    {
        await container.DisposeAsync();
    }
}
