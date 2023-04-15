﻿using Azure.Identity;
using Microsoft.Azure.Cosmos.Fluent;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using OhMyWord.Infrastructure.Options;
using OhMyWord.Infrastructure.Services;

namespace OhMyWord.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCosmosDbRepositories(this IServiceCollection services,
        HostBuilderContext context)
    {
        services.AddHttpClient();

        // options
        services.AddOptions<CosmosDbOptions>()
            .Bind(context.Configuration.GetSection(CosmosDbOptions.SectionName));

        // cosmos client
        services.AddSingleton(serviceProvider =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<CosmosDbOptions>>();
            var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();

            // use managed identity if account endpoint specified, otherwise use connection string
            var builder = string.IsNullOrEmpty(options.Value.AccountEndpoint)
                ? new CosmosClientBuilder(options.Value.ConnectionString)
                : new CosmosClientBuilder(options.Value.AccountEndpoint, new DefaultAzureCredential());

            return builder
                .WithApplicationName(options.Value.ApplicationName)
                .WithHttpClientFactory(() => httpClientFactory.CreateClient("CosmosDb"))
                .WithCustomSerializer(new EntitySerializer())
                .WithContentResponseOnWrite(false)
                .Build();
        });

        // repositories
        services.AddSingleton<IDefinitionsRepository, DefinitionsRepository>();
        services.AddSingleton<IPlayerRepository, PlayerRepository>();
        services.AddSingleton<IWordsRepository, WordsRepository>();

        return services;
    }

    public static IServiceCollection AddUsersRepository(this IServiceCollection services, HostBuilderContext context)
    {
        services.AddAzureClients(builder =>
        {
            if (context.HostingEnvironment.IsDevelopment())
            {
                // use local storage emulator
                builder.AddTableServiceClient("UseDevelopmentStorage=true");
            }
            else
            {
                // use managed identity
                builder.AddTableServiceClient(context.Configuration.GetSection("TableService"));
                builder.UseCredential(new DefaultAzureCredential());
            }
        });

        services.AddSingleton<IUsersRepository, UsersRepository>();

        return services;
    }
}
