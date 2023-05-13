using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OhMyWord.Infrastructure.Services.RapidApi;

namespace Infrastructure.Tests.Services.RapidApi;

public class RapidApiFixture
{
    public IServiceProvider ServiceProvider { get; }

    public RapidApiFixture()
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", false)
            .AddUserSecrets(typeof(RapidApiFixture).Assembly)
            .Build();

        ServiceProvider = new ServiceCollection()
            .AddLogging()
            .AddSingleton<IConfiguration>(configuration)
            .AddRapidApiServices()
            .BuildServiceProvider();
    }
}
