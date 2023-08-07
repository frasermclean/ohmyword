using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OhMyWord.Infrastructure.DependencyInjection;
using OhMyWord.Infrastructure.Services.RapidApi.IpGeoLocation;
using OhMyWord.Infrastructure.Services.RapidApi.WordsApi;

namespace OhMyWord.Infrastructure.Tests.Fixtures;

public class RapidApiFixture
{
    public IGeoLocationApiClient GeoLocationApiClient { get; }

    public IWordsApiClient WordsApiClient { get; }

    public RapidApiFixture()
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", false)
            .AddUserSecrets<RapidApiFixture>()
            .AddEnvironmentVariables("OhMyWord_")
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
