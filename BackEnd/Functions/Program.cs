using Azure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OhMyWord.Data.Options;
using OhMyWord.Data.Services;

namespace OhMyWord.Functions;

public static class Program
{
    public static void Main()
    {
        var host = new HostBuilder()
            .ConfigureFunctionsWorkerDefaults()            
            .ConfigureServices((context, collection) =>
            {
                collection.AddHttpClient();
                collection.AddOptions<CosmosDbOptions>()
                    .BindConfiguration(CosmosDbOptions.SectionName)
                    .ValidateDataAnnotations();

                collection.AddSingleton<ICosmosDbService, CosmosDbService>();
                collection.AddSingleton<IPlayerRepository, PlayerRepository>();

                // health checks
                var cosmosDbOptions = context.Configuration
                    .GetSection(CosmosDbOptions.SectionName)
                    .Get<CosmosDbOptions>();
                collection.AddHealthChecks()
                    .AddCosmosDb(cosmosDbOptions.ConnectionString);
            })
            .Build();

        host.Run();
    }
}
