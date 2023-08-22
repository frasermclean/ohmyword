using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using OhMyWord.Core.Models;
using OhMyWord.Core.Services;
using OhMyWord.Integrations.RapidApi.Models.IpGeoLocation;
using OhMyWord.Integrations.RapidApi.Services;
using System.Net;

namespace OhMyWord.Functions.Functions;

public class IpAddressFunctions
{
    private readonly ILogger<IpAddressFunctions> logger;
    private readonly IGeoLocationApiClient apiClient;
    private readonly IGeoLocationRepository repository;

    public IpAddressFunctions(ILogger<IpAddressFunctions> logger, IGeoLocationApiClient apiClient,
        IGeoLocationRepository repository)
    {
        this.logger = logger;
        this.apiClient = apiClient;
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
        var apiResponse = await apiClient.GetGeoLocationAsync(address, cancellationToken);
        geoLocation = MapToGeoLocation(apiResponse);
        await repository.AddGeoLocationAsync(geoLocation);
    }

    private static GeoLocation MapToGeoLocation(ApiResponse apiResponse) => new()
    {
        IpAddress = IPAddress.TryParse(apiResponse.IpAddress, out var ipAddress) ? ipAddress : IPAddress.None,
        CountryName = apiResponse.Country.Name ?? string.Empty,
        City = apiResponse.City.Name ?? string.Empty,
        CountryCode = apiResponse.Country.Code ?? string.Empty,
        LastUpdated = DateTime.UtcNow
    };
}
