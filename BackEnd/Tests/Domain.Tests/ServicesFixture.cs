using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OhMyWord.Domain.DependencyInjection;
using OhMyWord.Domain.Services;
using OhMyWord.Infrastructure.DependencyInjection;

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
                collection.AddDomainServices(context.Configuration);
                collection.AddRapidApiServices();
                collection.AddTableRepositories(context.Configuration);
            })
            .Build();

        GeoLocationService = host.Services.GetRequiredService<IGeoLocationService>();
    }
}
