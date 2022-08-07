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
            .ConfigureAppConfiguration((context, builder) =>
            {                
                // add Azure app configuration
                builder.AddAzureAppConfiguration(options =>
                {
                    var endpoint = context.Configuration.GetValue<string>("APP_CONFIG_ENDPOINT");
                    var uri = new Uri(endpoint);
                    options.Connect(uri, new DefaultAzureCredential());
                });
            })
            .ConfigureServices((context, collection) =>
            {
                collection.AddHttpClient();
                collection.Configure<CosmosDbOptions>(context.Configuration.GetSection(CosmosDbOptions.SectionName));                
                collection.AddSingleton<ICosmosDbService, CosmosDbService>();
                collection.AddSingleton<IPlayerRepository, PlayerRepository>();
            })
            .Build();

        host.Run();
    }
}
