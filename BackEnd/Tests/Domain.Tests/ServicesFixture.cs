using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OhMyWord.Domain.Extensions;
using OhMyWord.Domain.Services;
using OhMyWord.Infrastructure.Extensions;
using OhMyWord.Infrastructure.Services.RapidApi;

namespace Domain.Tests;

public class ServicesFixture
{
    public IGeoLocationService GeoLocationService { get; }

    public ServicesFixture()
    {
        var host = new HostBuilder()
            .UseEnvironment("Development")
            .ConfigureAppConfiguration(builder =>
            {
                builder.AddJsonFile("appsettings.json", false);
                builder.AddUserSecrets(typeof(ServicesFixture).Assembly);
            })
            .ConfigureServices((context, collection) =>
            {
                collection.AddDomainServices();
                collection.AddRapidApiServices();
                collection.AddTableRepositories(context);
            })
            .Build();

        GeoLocationService = host.Services.GetRequiredService<IGeoLocationService>();
    }
}
