using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OhMyWord.Data.Options;
using OhMyWord.Data.Services;

namespace OhMyWord.Data.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDataServices(this IServiceCollection services, IConfiguration configuration)
    {
        // cosmos db related services
        services.AddHttpClient();
        services.Configure<CosmosDbOptions>(configuration.GetSection(CosmosDbOptions.SectionName));
        services.AddSingleton<IDatabaseService, DatabaseService>();
        
        // repositories
        services.AddSingleton<IDefinitionsRepository, DefinitionsRepository>();
        services.AddSingleton<IPlayerRepository, PlayerRepository>();
        services.AddSingleton<IWordsRepository, WordsRepository>();        
        
        return services;
    }
}
