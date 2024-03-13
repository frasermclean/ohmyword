using Azure.Identity;

namespace OhMyWord.Api.Startup;

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
        var connectionString = configuration["TableService:ConnectionString"];
        var endpoint = configuration["TableService:Endpoint"] ?? string.Empty;

        foreach (var table in TableStorageTables)
        {
            var checkName = $"table-{table}";

            if (!string.IsNullOrEmpty(connectionString))
                builder.AddAzureTable(connectionString, table, checkName);
            else if (!string.IsNullOrEmpty(endpoint))
                builder.AddAzureTable(new Uri(endpoint), new DefaultAzureCredential(), table, checkName);
        }

        return builder;
    }
}
