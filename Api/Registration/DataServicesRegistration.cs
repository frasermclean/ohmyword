using OhMyWord.Services;
using OhMyWord.Services.Options;
using OhMyWord.Services.Repositories;

namespace OhMyWord.Api.Registration;

public static class DataServicesRegistration
{
    public static IServiceCollection AddCosmosDbService(this IServiceCollection services, ConfigurationManager configuration)
    {
        // add configuration options
        services.AddOptions<CosmosDbOptions>()
            .Bind(configuration.GetSection("CosmosDb"))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddHttpClient();
        services.AddSingleton<ICosmosDbService, CosmosDbService>();

        return services;
    }

    public static IServiceCollection AddRepositoryServices(this IServiceCollection services)
    {
        services.AddSingleton<IWordsRepository, WordsRepository>();
        services.AddSingleton<IPlayerRepository, PlayerRepository>();

        return services;
    }
}
