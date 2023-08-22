using FluentResults;
using OhMyWord.Core.Models;
using OhMyWord.Core.Services;
using OhMyWord.Integrations.Errors;
using System.Net;

namespace OhMyWord.Domain.Services;

public interface IGeoLocationService
{
    Task<Result<GeoLocation>> GetGeoLocationAsync(string address, CancellationToken cancellationToken = default);

    Task<Result<GeoLocation>> GetGeoLocationAsync(IPAddress ipAddress, CancellationToken cancellationToken = default);
}

public class GeoLocationService : IGeoLocationService
{
    private readonly IGeoLocationRepository repository;
    private readonly IGeoLocationClient client;

    public GeoLocationService(IGeoLocationRepository repository, IGeoLocationClient client)
    {
        this.repository = repository;
        this.client = client;
    }

    public async Task<Result<GeoLocation>> GetGeoLocationAsync(string address,
        CancellationToken cancellationToken = default) => IPAddress.TryParse(address, out var ipAddress)
        ? await GetGeoLocationAsync(ipAddress, cancellationToken)
        : new InvalidIpAddressError(address);

    public async Task<Result<GeoLocation>> GetGeoLocationAsync(IPAddress ipAddress,
        CancellationToken cancellationToken = default)
    {
        // lookup in table storage
        var geoLocation = await repository.GetGeoLocationAsync(ipAddress, cancellationToken);
        if (geoLocation is not null)
            return geoLocation;

        // lookup in API and store in table storage
        geoLocation = await client.GetGeoLocationAsync(ipAddress, cancellationToken);
        await repository.AddGeoLocationAsync(geoLocation);

        return geoLocation;
    }
}
