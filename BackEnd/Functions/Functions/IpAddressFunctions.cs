using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace OhMyWord.Functions.Functions;

public class IpAddressFunctions
{
    private readonly ILogger<IpAddressFunctions> logger;
    //private readonly IGeoLocationService geoLocationService;

    public IpAddressFunctions(ILogger<IpAddressFunctions> logger)
    {
        this.logger = logger;
    }

    [Function("ProcessIpAddress")]
    public async Task ProcessIpAddress(
        [ServiceBusTrigger("%ServiceBus:IpLookupQueueName%", Connection = "ServiceBus")]
        string ipAddress)
    {
        logger.LogInformation("Processing IP address: {IpAddress}", ipAddress);
        //await geoLocationService.GetGeoLocationAsync(ipAddress);
    }
}
