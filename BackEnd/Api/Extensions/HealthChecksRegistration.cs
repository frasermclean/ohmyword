using Azure.Identity;
using OhMyWord.WordsApi.HealthChecks;

namespace OhMyWord.Api.Extensions;

public static class HealthChecksRegistration
{
    public static IServiceCollection AddApplicationHealthChecks(this IServiceCollection services,
        HostBuilderContext context)
    {
        // local development health checks
        if (context.HostingEnvironment.IsDevelopment())
        {
            services.AddHealthChecks()
                .AddCheck<WordsApiClientHealthCheck>("WordsApiClient");

            return services;
        }

        // production health checks
        services.AddHealthChecks()
            .AddCosmosDbCollection(
                context.Configuration["CosmosDb:AccountEndpoint"] ?? string.Empty,
                new DefaultAzureCredential(),
                context.Configuration.GetValue<string>("CosmosDb:DatabaseId"),
                context.Configuration.GetValue<string[]>("CosmosDb:ContainerIds"))
            .AddAzureTable(new Uri(context.Configuration["TableService:Endpoint"] ?? string.Empty),
                new DefaultAzureCredential(), "users")
            .AddCheck<WordsApiClientHealthCheck>("WordsApiClient");

        return services;
    }
}
