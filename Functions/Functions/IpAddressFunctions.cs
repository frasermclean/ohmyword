using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace OhMyWord.Functions.Functions;

public class IpAddressFunctions
{
    private readonly ILogger<IpAddressFunctions> logger;

    public IpAddressFunctions(ILogger<IpAddressFunctions> logger)
    {
        this.logger = logger;
    }

    [Function("ProcessIpAddress")]
    public Task ProcessIpAddress(
        [ServiceBusTrigger("%ServiceBus:IpLookupQueueName%", Connection = "ServiceBus")]
        string ipAddress,
        FunctionContext context)
    {
        logger.LogInformation("Processing IP address {IpAddress}", ipAddress);
        return Task.CompletedTask;
    }
}
