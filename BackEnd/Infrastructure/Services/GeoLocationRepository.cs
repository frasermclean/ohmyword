using Azure.Data.Tables;
using Microsoft.Extensions.Logging;
using OhMyWord.Infrastructure.Models;

namespace OhMyWord.Infrastructure.Services;

public interface IGeoLocationRepository
{
    Task<GeoLocationEntity?> GetGeoLocationAsync(string ipAddress, CancellationToken cancellationToken = default);
    Task AddGeoLocationAsync(GeoLocationEntity geoLocationEntity);
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
        var response = await tableClient.GetEntityAsync<GeoLocationEntity>(partitionKey, ipAddress,
            cancellationToken: cancellationToken);

        if (response.HasValue)
            logger.LogInformation("GeoLocation for IP address: {IpAddress} was found", ipAddress);
        else
            logger.LogWarning("GeoLocation for IP address: {IpAddress} was not found", ipAddress);

        return response;
    }

    public Task AddGeoLocationAsync(GeoLocationEntity geoLocationEntity)
    {
        throw new NotImplementedException();
    }
}
