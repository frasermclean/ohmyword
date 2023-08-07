using Azure.Identity;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OhMyWord.Infrastructure.Options;
using OhMyWord.Infrastructure.Services.Messaging;

namespace OhMyWord.Infrastructure.DependencyInjection;

public static class MessagingServicesRegistration
{
    public static IServiceCollection AddMessagingServices(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddAzureClients(builder =>
        {
            builder.AddServiceBusClientWithNamespace(configuration["ServiceBus:Namespace"]);
            builder.UseCredential(new DefaultAzureCredential());
        });

        services.AddOptions<MessagingOptions>()
            .Bind(configuration.GetSection(MessagingOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddScoped<IIpAddressMessageSender, IpAddressMessageSender>();

        return services;
    }
}
