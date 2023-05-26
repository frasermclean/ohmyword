using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.FeatureManagement;
using OhMyWord.Domain.Contracts.Notifications;
using OhMyWord.Domain.Options;
using OhMyWord.Infrastructure.Services.Messaging;

namespace OhMyWord.Domain.Contracts.Handlers;

public class PlayerConnectedHandler : INotificationHandler<PlayerConnectedNotification>
{
    private readonly ILogger<PlayerConnectedHandler> logger;
    private readonly IFeatureManager featureManager;
    private readonly IIpAddressMessageSender ipAddressMessageSender;

    public PlayerConnectedHandler(ILogger<PlayerConnectedHandler> logger, IFeatureManager featureManager,
        IIpAddressMessageSender ipAddressMessageSender)
    {
        this.logger = logger;
        this.featureManager = featureManager;
        this.ipAddressMessageSender = ipAddressMessageSender;
    }

    public async Task Handle(PlayerConnectedNotification notification, CancellationToken cancellationToken)
    {
        logger.LogInformation("Player connected from {IpAddress}, with connection ID: {ConnectionId}",
            notification.IpAddress, notification.ConnectionId);

        var isFeatureEnabled = await featureManager.IsEnabledAsync(FeatureFlags.IpLookup);

        if (isFeatureEnabled)
        {
            await ipAddressMessageSender.SendIpLookupMessageAsync(notification.IpAddress);
        }
    }
}
