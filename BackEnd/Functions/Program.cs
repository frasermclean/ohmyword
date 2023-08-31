using Azure.Core.Serialization;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OhMyWord.Integrations.RapidApi.DependencyInjection;
using OhMyWord.Integrations.Storage.DependencyInjection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OhMyWord.Functions;

public static class Program
{
    public static void Main()
    {
        var host = new HostBuilder()
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
                services.AddRapidApiServices();
                services.AddTableRepositories(context.Configuration);

                // application insights
                services.AddApplicationInsightsTelemetryWorkerService();
                services.ConfigureFunctionsApplicationInsights();
            })
            .Build();

        host.Run();
    }
}
