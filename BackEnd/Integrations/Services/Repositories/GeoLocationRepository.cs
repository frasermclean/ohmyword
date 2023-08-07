using Azure;
using Azure.Data.Tables;
using Microsoft.Extensions.Logging;
using OhMyWord.Infrastructure.Models.Entities;
using System.Net;
using System.Net.Sockets;

namespace OhMyWord.Infrastructure.Services.Repositories;

public interface IGeoLocationRepository
{
    Task<GeoLocationEntity?> GetGeoLocationAsync(IPAddress ipAddress, CancellationToken cancellationToken = default);
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

    public async Task<GeoLocationEntity?> GetGeoLocationAsync(IPAddress ipAddress, CancellationToken cancellationToken)
    {
        var partitionKey = ipAddress.AddressFamily == AddressFamily.InterNetworkV6 ? "IPv6" : "IPv4";
        var rowKey = ipAddress.ToString();

        try
        {
            var result = await tableClient.GetEntityAsync<GeoLocationEntity>(partitionKey, rowKey,
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
