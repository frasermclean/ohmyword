using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OhMyWord.Integrations.ServiceBus.Options;

namespace OhMyWord.Integrations.ServiceBus.Services;

public interface IServiceBusMessageSender
{
    Task SendIpLookupMessageAsync(string ipAddress);
}

public sealed class ServiceBusMessageSender : IServiceBusMessageSender
{
    private readonly ServiceBusClient serviceBusClient;
    private readonly ILogger<ServiceBusMessageSender> logger;
    private readonly ServiceBusOptions options;

    public ServiceBusMessageSender(ServiceBusClient serviceBusClient, IOptions<ServiceBusOptions> options,
        ILogger<ServiceBusMessageSender> logger)
    {
        this.serviceBusClient = serviceBusClient;
        this.logger = logger;
        this.options = options.Value;
    }

    public async Task SendIpLookupMessageAsync(string ipAddress)
    {
        logger.LogInformation("Sending IP lookup message for address: {IpAddress}", ipAddress);

        await using var sender = serviceBusClient.CreateSender(options.IpLookupQueueName);
        await sender.SendMessageAsync(new ServiceBusMessage(ipAddress));
    }
}
