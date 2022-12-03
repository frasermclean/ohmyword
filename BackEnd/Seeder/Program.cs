using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OhMyWord.Data.Options;
using OhMyWord.Data.Services;

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
                services.Configure<CosmosDbOptions>(configuration.GetSection(CosmosDbOptions.SectionName));
                services.AddSingleton<IDatabaseService, DatabaseService>();
                services.AddSingleton<IWordsRepository, WordsRepository>();

                // local application services
                services.AddSingleton<DataReader>();
                services.AddHostedService<MainService>();
            })
            .Build();

        await host.RunAsync();
    }
}
