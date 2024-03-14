using Azure.Identity;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OhMyWord.Integrations.ServiceBus.Options;
using OhMyWord.Integrations.ServiceBus.Services;

namespace OhMyWord.Integrations.ServiceBus.DependencyInjection;

public static class ServiceBusServicesRegistration
{
    public static IServiceCollection AddServiceBusServices(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddAzureClients(builder =>
        {
            builder.AddServiceBusClientWithNamespace(configuration["ServiceBus:Namespace"]);
            builder.UseCredential(new DefaultAzureCredential());
        });

        services.AddOptions<ServiceBusOptions>()
            .Bind(configuration.GetSection(ServiceBusOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddScoped<IServiceBusMessageSender, ServiceBusMessageSender>();

        return services;
    }
}
