using Azure.Core.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OhMyWord.Infrastructure.Extensions;
using OhMyWord.Infrastructure.Options;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OhMyWord.Functions;

public static class Program
{
    public static void Main()
    {
        var host = new HostBuilder()
            .ConfigureFunctionsWorkerDefaults(builder =>
            {
                builder.Serializer = new JsonObjectSerializer(new JsonSerializerOptions
                {
                    IgnoreReadOnlyProperties = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
                    Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
                });
            })
            .ConfigureAppConfiguration(builder =>
            {
                builder.AddJsonFile("local.settings.json", optional: true);
            })
            .ConfigureServices((context, services) =>
            {
                // infrastructure services
                services.AddInfrastructureServices(context.Configuration);

                // health checks
                var cosmosDbOptions = context.Configuration
                    .GetSection(CosmosDbOptions.SectionName)
                    .Get<CosmosDbOptions>();
                services.AddHealthChecks()
                    .AddCosmosDb(cosmosDbOptions?.ConnectionString ?? string.Empty);
            })
            .Build();

        host.Run();
    }
}
