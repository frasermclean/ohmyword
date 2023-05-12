using OhMyWord.Infrastructure.Services.Messaging;

namespace OhMyWord.Api.Events.PlayerConnected;

public class PlayerConnectedHandler : IEventHandler<PlayerConnectedEvent>
{
    private readonly IServiceScopeFactory serviceScopeFactory;

    public PlayerConnectedHandler(IServiceScopeFactory serviceScopeFactory)
    {
        this.serviceScopeFactory = serviceScopeFactory;
    }

    public async Task HandleAsync(PlayerConnectedEvent connectedEvent, CancellationToken cancellationToken)
    {
        await using var scope = serviceScopeFactory.CreateAsyncScope();
        var messageSender = scope.Resolve<IIpAddressMessageSender>();
        await messageSender.SendIpLookupMessageAsync(connectedEvent.IpAddress);
    }
}
