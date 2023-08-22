using Azure.Identity;

namespace OhMyWord.Api.Startup;

public static class HealthChecksRegistration
{
    public static IServiceCollection AddApplicationHealthChecks(this IServiceCollection services,
        IConfiguration configuration)
    {
        var builder = services.AddHealthChecks();
        builder.AddDatabaseChecks(configuration);
        builder.AddTableStorageChecks(configuration);

        return services;
    }

    private static readonly IEnumerable<string> DataBaseContainers = new[] { "words", "definitions", "players" };

    private static void AddDatabaseChecks(this IHealthChecksBuilder builder,
        IConfiguration configuration)
    {
        var accountEndpoint = configuration["CosmosDb:AccountEndpoint"];
        var connectionString = configuration["CosmosDb:ConnectionString"] ?? string.Empty;
        var databaseId = configuration.GetValue<string>("CosmosDb:DatabaseId") ?? string.Empty;

        if (string.IsNullOrEmpty(accountEndpoint))
            builder.AddCosmosDbCollection(connectionString, databaseId, DataBaseContainers);
        else
            builder.AddCosmosDbCollection(accountEndpoint, new DefaultAzureCredential(), databaseId,
                DataBaseContainers);
    }

    private static readonly IEnumerable<string> TableStorageTables = new[] { "users", "geoLocations" };

    private static void AddTableStorageChecks(this IHealthChecksBuilder builder,
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
    }
}
