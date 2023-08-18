using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OhMyWord.Integrations.Options;

namespace OhMyWord.Integrations.Services.Messaging;

public interface IMessageSender
{
    Task SendIpLookupMessageAsync(string ipAddress);
}

public sealed class MessageSender : IMessageSender
{
    private readonly ServiceBusClient serviceBusClient;
    private readonly ILogger<MessageSender> logger;
    private readonly MessagingOptions options;

    public MessageSender(ServiceBusClient serviceBusClient, IOptions<MessagingOptions> options,
        ILogger<MessageSender> logger)
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
