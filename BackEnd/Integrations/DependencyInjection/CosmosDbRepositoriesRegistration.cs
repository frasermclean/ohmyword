using Azure.Identity;
using Microsoft.Azure.Cosmos.Fluent;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OhMyWord.Integrations.Options;
using OhMyWord.Integrations.Services.Repositories;

namespace OhMyWord.Integrations.DependencyInjection;

public static class CosmosDbRepositoriesRegistration
{
    public static IServiceCollection AddCosmosDbRepositories(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddHttpClient();

        // options
        services.AddOptions<CosmosDbOptions>()
            .Bind(configuration.GetSection(CosmosDbOptions.SectionName))
            .Validate(CosmosDbOptions.Validate, "Invalid CosmosDb configuration")
            .ValidateOnStart();

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
                .WithCustomSerializer(EntitySerializer.Instance)
                .WithContentResponseOnWrite(true)
                .Build();
        });

        // repositories
        services.AddSingleton<IDefinitionsRepository, DefinitionsRepository>();
        services.AddSingleton<IPlayerRepository, PlayerRepository>();
        services.AddSingleton<IRoundsRepository, RoundsRepository>();
        services.AddSingleton<ISessionsRepository, SessionsRepository>();
        services.AddSingleton<IWordsRepository, WordsRepository>();

        return services;
    }
}
