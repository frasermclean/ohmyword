using MediatR;
using Microsoft.Extensions.Logging;
using OhMyWord.Domain.Contracts.Notifications;
using OhMyWord.Infrastructure.Services.Messaging;

namespace OhMyWord.Domain.Contracts.Handlers;

public class PlayerConnectedHandler : INotificationHandler<PlayerConnectedNotification>
{
    private readonly ILogger<PlayerConnectedHandler> logger;
    private readonly IIpAddressMessageSender ipAddressMessageSender;

    public PlayerConnectedHandler(ILogger<PlayerConnectedHandler> logger,
        IIpAddressMessageSender ipAddressMessageSender)
    {
        this.logger = logger;
        this.ipAddressMessageSender = ipAddressMessageSender;
    }

    public async Task Handle(PlayerConnectedNotification notification, CancellationToken cancellationToken)
    {
        logger.LogInformation("Player connected from {IpAddress}, with connection ID: {ConnectionId}",
            notification.IpAddress, notification.ConnectionId);
        
        await ipAddressMessageSender.SendIpLookupMessageAsync(notification.IpAddress);
    }
}
