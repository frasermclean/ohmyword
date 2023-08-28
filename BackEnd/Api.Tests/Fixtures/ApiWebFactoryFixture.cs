using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OhMyWord.Data.CosmosDb.Options;
using OhMyWord.Data.CosmosDb.Tests.Fixtures;

namespace OhMyWord.Api.Tests.Fixtures;

public sealed class ApiWebFactoryFixture : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly CosmosDbEmulatorFixture cosmosDbEmulatorFixture = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // remove logging
        builder.ConfigureLogging(loggingBuilder => loggingBuilder.ClearProviders());

        // disable authorization for testing
        builder.UseSetting("FeatureManagement:Authorization", "false");

        builder.ConfigureTestServices(collection =>
        {
            // replace cosmos client with emulator client
            collection.RemoveAll<CosmosClient>();
            collection.RemoveAll<IOptions<CosmosDbOptions>>();
            collection.AddSingleton(cosmosDbEmulatorFixture.CosmosClient);
            collection.AddSingleton(cosmosDbEmulatorFixture.Options);
        });
    }

    async Task IAsyncLifetime.InitializeAsync()
    {
        await cosmosDbEmulatorFixture.InitializeAsync();
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await cosmosDbEmulatorFixture.DisposeAsync();
    }
}
