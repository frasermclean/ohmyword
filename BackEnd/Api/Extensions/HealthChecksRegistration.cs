using Azure.Identity;
using OhMyWord.Infrastructure.Services.RapidApi.WordsApi;

namespace OhMyWord.Api.Extensions;

public static class HealthChecksRegistration
{
    public static IServiceCollection AddApplicationHealthChecks(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddHealthChecks()
            .AddDatabaseChecks(configuration)
            .AddTableStorageChecks(configuration);

        return services;
    }

    private static readonly IEnumerable<string> DataBaseContainers = new[] { "words", "definitions", "players" };

    private static IHealthChecksBuilder AddDatabaseChecks(this IHealthChecksBuilder builder,
        IConfiguration configuration)
    {
        var accountEndpoint = configuration["CosmosDb:AccountEndpoint"];
        var connectionString = configuration["CosmosDb:ConnectionString"] ?? string.Empty;
        var databaseId = configuration.GetValue<string>("CosmosDb:DatabaseId") ?? string.Empty;

        return !string.IsNullOrEmpty(accountEndpoint)
            ? builder.AddCosmosDbCollection(accountEndpoint, new DefaultAzureCredential(), databaseId,
                DataBaseContainers)
            : builder.AddCosmosDbCollection(connectionString, databaseId, DataBaseContainers);
    }

    private static readonly IEnumerable<string> TableStorageTables = new[] { "users", "geoLocations" };

    private static IHealthChecksBuilder AddTableStorageChecks(this IHealthChecksBuilder builder,
        IConfiguration configuration)
    {
        var serviceEndpoint = configuration["TableService:Endpoint"] ?? string.Empty;

        if (string.IsNullOrEmpty(serviceEndpoint)) return builder;

        foreach (var table in TableStorageTables)
        {
            builder.AddAzureTable(new Uri(serviceEndpoint),
                new DefaultAzureCredential(), table, $"table-{table}");
        }

        return builder;
    }
}
