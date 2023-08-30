using Azure.Core.Serialization;
using Azure.Identity;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OhMyWord.Domain.DependencyInjection;
using OhMyWord.Integrations.DependencyInjection;
using OhMyWord.Integrations.RapidApi.DependencyInjection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OhMyWord.Functions;

public static class Program
{
    public static void Main()
    {
        var host = new HostBuilder()
            .ConfigureAppConfiguration((_, builder) =>
            {
                builder.AddJsonFile("appsettings.json");
            })
            .ConfigureFunctionsWorkerDefaults(options =>
            {
                options.Serializer = new JsonObjectSerializer(new JsonSerializerOptions
                {
                    IgnoreReadOnlyProperties = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
                    Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
                });
            })
            .ConfigureServices((context, services) =>
            {
                services.AddDomainServices(context.Configuration);
                services.AddRapidApiServices();
                services.AddTableRepositories(context.Configuration);

                // application insights
                services.AddApplicationInsightsTelemetryWorkerService();
                services.ConfigureFunctionsApplicationInsights();

                // health checks
                if (context.HostingEnvironment.IsDevelopment())
                {
                    services.AddHealthChecks()
                        .AddAzureTable("UseDevelopmentStorage=true", "users");
                }
                else
                {
                    var endpointUri = new Uri(context.Configuration["TableService:Endpoint"] ?? string.Empty);
                    var credential = new DefaultAzureCredential();
                    services.AddHealthChecks()
                        .AddAzureTable(endpointUri, credential, "users"); // TODO: Refactor functions health checks
                }
            })
            .Build();

        host.Run();
    }
}
