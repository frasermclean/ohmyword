using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OhMyWord.Services.Data;
using OhMyWord.Services.Data.Repositories;
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

                // cosmos db related services
                services.AddHttpClient();
                services.AddOptions<CosmosDbOptions>().Bind(configuration.GetSection("CosmosDb"));
                services.AddSingleton<ICosmosDbService, CosmosDbService>();
                services.AddSingleton<IWordsRepository, WordsRepository>();

                // local application services
                services.AddSingleton<DataReader>();
                services.AddHostedService<MainService>();
            })
            .Build();

        await host.RunAsync();
    }
}
