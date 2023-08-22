using Azure.Data.Tables;
using Microsoft.Extensions.Logging;
using OhMyWord.Core.Models;
using OhMyWord.Core.Services;
using OhMyWord.Data.Tables.Models;
using System.Net;
using System.Net.Sockets;

namespace OhMyWord.Data.Tables.Services;

public class GeoLocationRepository : IGeoLocationRepository
{
    private readonly ILogger<GeoLocationRepository> logger;
    private readonly TableClient tableClient;

    public GeoLocationRepository(ILogger<GeoLocationRepository> logger, TableServiceClient tableServiceClient)
    {
        this.logger = logger;
        tableClient = tableServiceClient.GetTableClient("geoLocations");
    }

    public async Task<GeoLocation?> GetGeoLocationAsync(string address, CancellationToken cancellationToken = default)
    {
        // parse address string
        if (IPAddress.TryParse(address, out var ipAddress))
        {
            return await GetGeoLocationAsync(ipAddress, cancellationToken);
        }

        logger.LogError("Invalid IP address: {IpAddress}", address);
        return null;
    }

    public async Task<GeoLocation?> GetGeoLocationAsync(IPAddress ipAddress, CancellationToken cancellationToken)
    {
        var partitionKey = ipAddress.AddressFamily == AddressFamily.InterNetworkV6 ? "IPv6" : "IPv4";
        var rowKey = ipAddress.ToString();

        var response = await tableClient.GetEntityIfExistsAsync<GeoLocationEntity>(partitionKey, rowKey,
            cancellationToken: cancellationToken);

        if (!response.HasValue)
        {
            logger.LogWarning("GeoLocation for IP address: {IpAddress} was not found", ipAddress);
            return default;
        }

        logger.LogInformation("GeoLocation for IP address: {IpAddress} was found", ipAddress);
        return MapToGeoLocation(response.Value);
    }

    public async Task AddGeoLocationAsync(GeoLocation geoLocation)
    {
        var entity = MapToEntity(geoLocation);
        await tableClient.AddEntityAsync(entity);
        logger.LogInformation("GeoLocation for IP address: {IpAddress} was added", entity.RowKey);
    }

    private static GeoLocation MapToGeoLocation(GeoLocationEntity entity) => new()
    {
        IpAddress = IPAddress.TryParse(entity.RowKey, out var ipAddress) ? ipAddress : IPAddress.None,
        CountryCode = entity.CountryCode.ToLower(),
        CountryName = entity.CountryName,
        City = entity.City,
        LastUpdated = entity.Timestamp?.UtcDateTime ?? default
    };

    private static GeoLocationEntity MapToEntity(GeoLocation geoLocation) => new()
    {
        PartitionKey = geoLocation.IpAddress.AddressFamily == AddressFamily.InterNetworkV6 ? "IPv6" : "IPv4",
        RowKey = geoLocation.IpAddress.ToString(),
        CountryCode = geoLocation.CountryCode.ToUpper(),
        CountryName = geoLocation.CountryName,
        City = geoLocation.City
    };
}
