using Azure.Identity;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OhMyWord.Integrations.Options;
using OhMyWord.Integrations.Services.Messaging;

namespace OhMyWord.Integrations.DependencyInjection;

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

        services.AddScoped<IMessageSender, MessageSender>();

        return services;
    }
}
