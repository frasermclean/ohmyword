using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OhMyWord.Data.Options;
using OhMyWord.Data.Services;

[assembly: FunctionsStartup(typeof(OhMyWord.Functions.Startup))]

namespace OhMyWord.Functions;

public class Startup : FunctionsStartup
{
    public override void Configure(IFunctionsHostBuilder builder)
    {
        var appConfigConnectionString = builder.GetContext().Configuration.GetConnectionString("AzureAppConfiguration");
        var configuration = BuildConfiguration(builder.GetContext().ApplicationRootPath, appConfigConnectionString);
        
        builder.Services.Configure<CosmosDbOptions>(configuration.GetSection(CosmosDbOptions.SectionName));
        builder.Services.AddSingleton<ICosmosDbService, CosmosDbService>();
        builder.Services.AddSingleton<IPlayerRepository, PlayerRepository>();
    }

    private static IConfiguration BuildConfiguration(string applicationRootPath, string appConfigConnectionString) =>
        new ConfigurationBuilder()
            .SetBasePath(applicationRootPath)
            .AddJsonFile("local.settings.json", true, false)
            .AddAzureAppConfiguration(appConfigConnectionString)            
            .Build();
}
