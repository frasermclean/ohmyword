using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OhMyWord.Services.Data;
using OhMyWord.Services.Options;

namespace OhMyWord.Seeder;

public static class Program
{
    public static async Task Main(string[] args)
    {
        var host = Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
            {
                var configuration = context.Configuration;

                services.AddOptions<CosmosDbOptions>().Bind(configuration.GetSection("CosmosDb"));
                services.AddHttpClient();
                services.AddSingleton<ICosmosDbService, CosmosDbService>();

                services.AddHostedService<MainService>();
            })
            .Build();

        await host.RunAsync();
    }
}
