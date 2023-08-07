using System.Text.Json;
using System.Text.Json.Serialization;

namespace OhMyWord.Api.Startup;

public static class SignalRServicesRegistration
{
    public static IServiceCollection AddSignalRServices(this IServiceCollection services,
        IConfiguration configuration)
    {
        var builder = services.AddSignalR()
            .AddJsonProtocol(options =>
            {
                var converter = new JsonStringEnumConverter(JsonNamingPolicy.CamelCase);
                options.PayloadSerializerOptions.Converters.Add(converter);
            });

        // add azure signalR service if connection string is present
        var connectionString = configuration["SignalRService:ConnectionString"];
        if (!string.IsNullOrEmpty(connectionString))
        {
            builder.AddAzureSignalR(connectionString);
        }

        return services;
    }
}
