using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace OhMyWord.Integrations.Tests.Fixtures;

public sealed class GraphApiClientFixture
{
    public GraphApiClientFixture()
    {
        var host = new HostBuilder()
            .UseEnvironment("Development")
            .ConfigureAppConfiguration(builder =>
            {
                builder.AddJsonFile("appsettings.json", false);
                builder.AddUserSecrets<GraphApiClientFixture>();
                builder.AddEnvironmentVariables("OhMyWord_");
            })
            .ConfigureServices((context, collection) =>
            {
                collection.AddGraphApiClient(context.Configuration);
            })
            .Build();

        GraphApiClient = host.Services.GetRequiredService<IGraphApiClient>();
    }

    public IGraphApiClient GraphApiClient { get; }
}
