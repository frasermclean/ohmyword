using Azure.Identity;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OhMyWord.Infrastructure.Options;
using OhMyWord.Infrastructure.Services.Messaging;

namespace OhMyWord.Infrastructure.DependencyInjection;

public static class MessagingServicesRegistration
{
    public static IServiceCollection AddMessagingServices(this IServiceCollection services, HostBuilderContext context)
    {
        services.AddAzureClients(builder =>
        {
            builder.AddServiceBusClientWithNamespace(context.Configuration["ServiceBus:Namespace"]);
            builder.UseCredential(new DefaultAzureCredential());
        });

        services.AddOptions<MessagingOptions>()
            .Bind(context.Configuration.GetSection(MessagingOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddScoped<IIpAddressMessageSender, IpAddressMessageSender>();

        return services;
    }
}
