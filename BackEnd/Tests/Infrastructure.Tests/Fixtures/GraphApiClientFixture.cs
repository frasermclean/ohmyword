using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OhMyWord.Infrastructure.DependencyInjection;
using OhMyWord.Infrastructure.Services.GraphApi;
using OhMyWord.Infrastructure.Tests.Services.GraphApi;

namespace OhMyWord.Infrastructure.Tests.Fixtures;

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
