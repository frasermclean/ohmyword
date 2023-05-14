using Azure.Core.Serialization;
using Azure.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OhMyWord.Domain.Services;
using OhMyWord.Infrastructure.Extensions;
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
                services.AddSingleton<IUsersService, UsersService>();
                services.AddTableRepositories(context);

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
                        .AddAzureTable(endpointUri, credential, "users");
                }
            })
            .Build();

        host.Run();
    }
}
