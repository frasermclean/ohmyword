using WhatTheWord.Data;
using WhatTheWord.Data.Repositories;

namespace WhatTheWord.Api.Configuration;

public static class DataServices
{
    public static IServiceCollection AddCosmosDbService(this IServiceCollection services, ConfigurationManager configuration)
    {
        // add configuration options
        services.AddOptions<CosmosDbOptions>()
            .Bind(configuration.GetSection("CosmosDb"))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddSingleton<ICosmosDbService, CosmosDbService>();

        return services;
    }

    public static IServiceCollection AddRepositoryServices(this IServiceCollection services)
    {
        services.AddSingleton<IWordsRepository, WordsRepository>();

        return services;
    }
}
