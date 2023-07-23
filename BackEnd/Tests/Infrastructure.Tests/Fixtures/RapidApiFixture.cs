using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OhMyWord.Infrastructure.DependencyInjection;
using OhMyWord.Infrastructure.Services.RapidApi.IpGeoLocation;

namespace OhMyWord.Infrastructure.Tests.Fixtures;

public class RapidApiFixture
{
    public IGeoLocationApiClient GeoLocationApiClient { get; }    

    public RapidApiFixture()
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", false)            
            .AddUserSecrets(typeof(RapidApiFixture).Assembly)
            .Build();

        var serviceProvider = new ServiceCollection()
            .AddLogging()
            .AddSingleton<IConfiguration>(configuration)
            .AddRapidApiServices()
            .BuildServiceProvider();
        
        GeoLocationApiClient = serviceProvider.GetRequiredService<IGeoLocationApiClient>();
    }
}
