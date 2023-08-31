using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OhMyWord.Integrations.RapidApi.DependencyInjection;
using OhMyWord.Integrations.RapidApi.Services;

namespace OhMyWord.Integrations.RapidApi.Tests.Fixtures;

public sealed class RapidApiFixture
{
    public IGeoLocationApiClient GeoLocationApiClient { get; }

    public IWordsApiClient WordsApiClient { get; }

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

        GeoLocationApiClient = serviceProvider.GetRequiredService<IGeoLocationApiClient>();
        WordsApiClient = serviceProvider.GetRequiredService<IWordsApiClient>();
    }
}
