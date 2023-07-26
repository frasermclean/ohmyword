using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace OhMyWord.Api.Tests.Fixtures;

public class ApiWebFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // remove logging
        builder.ConfigureLogging(loggingBuilder => loggingBuilder.ClearProviders());

        // disable authorization for testing
        builder.UseSetting("FeatureManagement:Authorization", "false");

        builder.ConfigureTestServices(collection =>
        {
            collection.RemoveAll(typeof(CosmosClient));
        });
    }
}
