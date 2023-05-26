using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OhMyWord.Infrastructure.Options;
using System.Net;

namespace OhMyWord.Infrastructure.Services.Messaging;

public interface IIpAddressMessageSender
{
    Task SendIpLookupMessageAsync(IPAddress ipAddress);
}

public sealed class IpAddressMessageSender : IIpAddressMessageSender, IAsyncDisposable
{
    private readonly ILogger<IpAddressMessageSender> logger;
    private readonly ServiceBusSender sender;

    public IpAddressMessageSender(ServiceBusClient serviceBusClient, IOptions<MessagingOptions> options,
        ILogger<IpAddressMessageSender> logger)
    {
        this.logger = logger;
        sender = serviceBusClient.CreateSender(options.Value.IpLookupQueueName);
    }

    public Task SendIpLookupMessageAsync(IPAddress ipAddress)
    {
        var message = new ServiceBusMessage(ipAddress.ToString());
        logger.LogInformation("Sending IP lookup message for address: {IpAddress}", ipAddress);
        return sender.SendMessageAsync(message);
    }

    public ValueTask DisposeAsync() => sender.DisposeAsync();
}
