using Microsoft.Extensions.Logging;
using OhMyWord.Domain.Extensions;
using OhMyWord.Domain.Models;
using OhMyWord.Infrastructure.Services;
using OhMyWord.Infrastructure.Services.RapidApi.IpGeoLocation;
using System.Net;

namespace OhMyWord.Domain.Services;

public interface IGeoLocationService
{
    Task<GeoLocation> GetGeoLocationAsync(string ipAddress, CancellationToken cancellationToken = default);
}

public class GeoLocationService : IGeoLocationService
{
    private readonly ILogger<GeoLocationService> logger;
    private readonly IGeoLocationRepository repository;
    private readonly IGeoLocationApiClient apiClient;

    public GeoLocationService(ILogger<GeoLocationService> logger, IGeoLocationRepository repository,
        IGeoLocationApiClient apiClient)
    {
        this.logger = logger;
        this.repository = repository;
        this.apiClient = apiClient;
    }

    public async Task<GeoLocation> GetGeoLocationAsync(string ipAddress, CancellationToken cancellationToken)
    {
        // validate IP address
        if (!IPAddress.TryParse(ipAddress, out _))
        {
            logger.LogError("Invalid IP address: {IpAddress}", ipAddress);
            return GeoLocation.None;
        }

        // lookup in table storage
        var entity = await repository.GetGeoLocationAsync(ipAddress, cancellationToken);
        if (entity is not null) return entity.ToGeoLocation();

        // lookup in API and store in table storage
        entity = await apiClient.GetGetLocationAsync(ipAddress, cancellationToken);
        await repository.AddGeoLocationAsync(entity);
        return entity.ToGeoLocation();
    }
}
