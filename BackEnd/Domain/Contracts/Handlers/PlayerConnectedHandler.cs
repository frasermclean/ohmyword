using FastEndpoints;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.FeatureManagement;
using OhMyWord.Domain.Contracts.Events;
using OhMyWord.Domain.Options;
using OhMyWord.Integrations.ServiceBus.Services;

namespace OhMyWord.Domain.Contracts.Handlers;

public class PlayerConnectedHandler : IEventHandler<PlayerConnectedEvent>
{
    private readonly ILogger<PlayerConnectedHandler> logger;
    private readonly IFeatureManager featureManager;
    private readonly IServiceScopeFactory serviceScopeFactory;

    public PlayerConnectedHandler(ILogger<PlayerConnectedHandler> logger, IFeatureManager featureManager,
        IServiceScopeFactory serviceScopeFactory)
    {
        this.logger = logger;
        this.featureManager = featureManager;
        this.serviceScopeFactory = serviceScopeFactory;
    }

    public async Task HandleAsync(PlayerConnectedEvent eventModel, CancellationToken cancellationToken = new())
    {
        logger.LogInformation("Player connected from {IpAddress}, with connection ID: {ConnectionId}",
            eventModel.IpAddress, eventModel.ConnectionId);

        var isFeatureEnabled = await featureManager.IsEnabledAsync(FeatureFlags.IpLookup);

        if (isFeatureEnabled)
        {
            await using var scope = serviceScopeFactory.CreateAsyncScope();
            var messageSender = scope.ServiceProvider.GetRequiredService<IServiceBusMessageSender>();
            await messageSender.SendIpLookupMessageAsync(eventModel.IpAddress.ToString());
        }
    }
}
