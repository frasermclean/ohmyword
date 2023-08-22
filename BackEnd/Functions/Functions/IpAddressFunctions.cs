using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using OhMyWord.Core.Services;

namespace OhMyWord.Functions.Functions;

public class IpAddressFunctions
{
    private readonly ILogger<IpAddressFunctions> logger;
    private readonly IGeoLocationClient client;
    private readonly IGeoLocationRepository repository;

    public IpAddressFunctions(ILogger<IpAddressFunctions> logger, IGeoLocationClient client,
        IGeoLocationRepository repository)
    {
        this.logger = logger;
        this.client = client;
        this.repository = repository;
    }

    [Function("ProcessIpAddress")]
    public async Task ProcessIpAddress(
        [ServiceBusTrigger("%ServiceBus:IpLookupQueueName%", Connection = "ServiceBus")]
        string address, CancellationToken cancellationToken)
    {
        logger.LogInformation("Processing IP address: {IpAddress}", address);

        // lookup in table storage
        var geoLocation = await repository.GetGeoLocationAsync(address, cancellationToken);
        if (geoLocation is not null)
        {
            logger.LogInformation("GeoLocation for IP address: {IpAddress} already exists in table storage", address);
            return;
        }

        // lookup in API and store in table storage
        geoLocation = await client.GetGeoLocationAsync(address, cancellationToken);
        await repository.AddGeoLocationAsync(geoLocation);
    }
}
