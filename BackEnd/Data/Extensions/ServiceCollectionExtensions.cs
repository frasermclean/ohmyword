using Microsoft.Azure.Cosmos.Fluent;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OhMyWord.Data.Options;
using OhMyWord.Data.Services;

namespace OhMyWord.Data.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDataServices(this IServiceCollection services, IConfiguration configuration)
    {        
        services.AddHttpClient();
        
        // options
        services.AddOptions<CosmosDbOptions>()
            .Bind(configuration.GetSection(CosmosDbOptions.SectionName));
        
        // cosmos client
        services.AddSingleton(serviceProvider =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<CosmosDbOptions>>();
            var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();

            return new CosmosClientBuilder(options.Value.ConnectionString)
                .WithApplicationName(options.Value.ApplicationName)
                .WithHttpClientFactory(() => httpClientFactory.CreateClient("CosmosDb"))
                .WithCustomSerializer(new EntitySerializer())
                .WithContentResponseOnWrite(false)
                .Build();
        });

        // repositories
        services.AddSingleton<IDefinitionsRepository, DefinitionsRepository>();
        services.AddSingleton<IVisitorRepository, VisitorRepository>();
        services.AddSingleton<IUsersRepository, UsersRepository>();
        services.AddSingleton<IWordsRepository, WordsRepository>();

        return services;
    }
}
