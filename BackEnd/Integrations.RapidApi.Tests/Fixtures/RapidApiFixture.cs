using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OhMyWord.Core.Services;
using OhMyWord.Integrations.RapidApi.DependencyInjection;

namespace OhMyWord.Integrations.RapidApi.Tests.Fixtures;

public class RapidApiFixture
{
    public IGeoLocationClient GeoLocationApiClient { get; }

    public IDictionaryClient WordsApiClient { get; }

    public RapidApiFixture()
    {
        var configuration = new ConfigurationBuilder()
            .AddUserSecrets<RapidApiFixture>()
            .Build();

        var serviceProvider = new ServiceCollection()
            .AddLogging()
            .AddSingleton<IConfiguration>(configuration)
            .AddRapidApiServices()
            .BuildServiceProvider();

        GeoLocationApiClient = serviceProvider.GetRequiredService<IGeoLocationClient>();
        WordsApiClient = serviceProvider.GetRequiredService<IDictionaryClient>();
    }
}
