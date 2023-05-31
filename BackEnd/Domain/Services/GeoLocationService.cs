using OhMyWord.Domain.Extensions;
using OhMyWord.Domain.Models;
using OhMyWord.Infrastructure.Services;
using OhMyWord.Infrastructure.Services.RapidApi.IpGeoLocation;
using System.Net;

namespace OhMyWord.Domain.Services;

public interface IGeoLocationService
{
    Task<GeoLocation> GetGeoLocationAsync(string ipAddress, CancellationToken cancellationToken = default);
    Task<GeoLocation> GetGeoLocationAsync(IPAddress ipAddress, CancellationToken cancellationToken = default);
}

public class GeoLocationService : IGeoLocationService
{
    private readonly IGeoLocationRepository repository;
    private readonly IGeoLocationApiClient apiClient;

    public GeoLocationService(IGeoLocationRepository repository, IGeoLocationApiClient apiClient)
    {
        this.repository = repository;
        this.apiClient = apiClient;
    }

    public Task<GeoLocation> GetGeoLocationAsync(string ipAddress, CancellationToken cancellationToken = default)
        => GetGeoLocationAsync(IPAddress.Parse(ipAddress), cancellationToken);

    public async Task<GeoLocation> GetGeoLocationAsync(IPAddress ipAddress,
        CancellationToken cancellationToken = default)
    {
        // lookup in table storage
        var entity = await repository.GetGeoLocationAsync(ipAddress, cancellationToken);
        if (entity is not null) return entity.ToGeoLocation();

        // lookup in API and store in table storage
        entity = await apiClient.GetGetLocationAsync(ipAddress, cancellationToken);
        await repository.AddGeoLocationAsync(entity);
        return entity.ToGeoLocation();
    }
}
