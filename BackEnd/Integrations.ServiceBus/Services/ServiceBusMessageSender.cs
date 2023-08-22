using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OhMyWord.Core.Services;
using OhMyWord.Integrations.ServiceBus.Options;
using System.Net;

namespace OhMyWord.Integrations.ServiceBus.Services;

public sealed class ServiceBusMessageSender : IMessageSender
{
    private readonly ServiceBusClient client;
    private readonly ILogger<ServiceBusMessageSender> logger;
    private readonly ServiceBusOptions options;

    public ServiceBusMessageSender(ServiceBusClient client, IOptions<ServiceBusOptions> options,
        ILogger<ServiceBusMessageSender> logger)
    {
        this.client = client;
        this.logger = logger;
        this.options = options.Value;
    }

    public async Task SendIpLookupMessageAsync(IPAddress ipAddress)
    {
        logger.LogInformation("Sending IP lookup message for address: {IpAddress}", ipAddress);

        await using var sender = client.CreateSender(options.IpLookupQueueName);
        var message = new ServiceBusMessage(ipAddress.ToString());
        await sender.SendMessageAsync(message);
    }
}
