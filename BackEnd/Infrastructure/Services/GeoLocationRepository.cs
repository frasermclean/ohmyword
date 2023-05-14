using Azure;
using Azure.Data.Tables;
using Microsoft.Extensions.Logging;
using OhMyWord.Infrastructure.Models;
using OhMyWord.Infrastructure.Models.IpGeoLocation;

namespace OhMyWord.Infrastructure.Services;

public interface IGeoLocationRepository
{
    Task<GeoLocationEntity?> GetGeoLocationAsync(string ipAddress, CancellationToken cancellationToken = default);
    Task AddGeoLocationAsync(GeoLocationEntity entity);
}

public class GeoLocationRepository : IGeoLocationRepository
{
    private readonly ILogger<GeoLocationRepository> logger;
    private readonly TableClient tableClient;

    public GeoLocationRepository(ILogger<GeoLocationRepository> logger, TableServiceClient tableServiceClient)
    {
        this.logger = logger;
        tableClient = tableServiceClient.GetTableClient("geoLocations");
    }

    public async Task<GeoLocationEntity?> GetGeoLocationAsync(string ipAddress, CancellationToken cancellationToken)
    {
        var partitionKey = ipAddress.Contains(':') ? "IPv6" : "IPv4";

        try
        {
            var result = await tableClient.GetEntityAsync<GeoLocationEntity>(partitionKey, ipAddress,
                cancellationToken: cancellationToken);
            logger.LogInformation("GeoLocation for IP address: {IpAddress} was found", ipAddress);
            return result;
        }
        catch (RequestFailedException exception)
        {
            logger.LogWarning(exception, "GeoLocation for IP address: {IpAddress} was not found", ipAddress);
            return default;
        }
    }

    public async Task AddGeoLocationAsync(GeoLocationEntity entity)
    {
        await tableClient.AddEntityAsync(entity);
        logger.LogInformation("GeoLocation for IP address: {IpAddress} was added", entity.RowKey);
    }
}
