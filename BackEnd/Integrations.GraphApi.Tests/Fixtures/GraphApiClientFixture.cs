using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OhMyWord.Core.Services;
using OhMyWord.Integrations.GraphApi.DependencyInjection;

namespace OhMyWord.Integrations.GraphApi.Tests.Fixtures;

public sealed class GraphApiClientFixture
{
    public GraphApiClientFixture()
    {
        var host = new HostBuilder()
            .UseEnvironment("Development")
            .ConfigureAppConfiguration(builder =>
            {
                builder.AddJsonFile("appsettings.json");
                builder.AddUserSecrets<GraphApiClientFixture>();
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
