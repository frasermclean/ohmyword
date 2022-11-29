﻿using OhMyWord.Data.Options;
using OhMyWord.Data.Services;

namespace OhMyWord.Api.Registration;

public static class DataServicesRegistration
{
    public static void AddCosmosDbService(this IServiceCollection services, IConfiguration configuration)
    {
        // add configuration options
        services.AddOptions<CosmosDbOptions>()
            .Bind(configuration.GetSection(CosmosDbOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddHttpClient();
        services.AddSingleton<IDatabaseService, DatabaseService>();
    }

    public static IServiceCollection AddRepositoryServices(this IServiceCollection services)
    {
        services.AddSingleton<IWordsRepository, WordsRepository>();
        services.AddSingleton<IPlayerRepository, PlayerRepository>();

        return services;
    }
}