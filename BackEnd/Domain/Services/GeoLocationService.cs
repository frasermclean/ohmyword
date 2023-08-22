using FluentResults;
using OhMyWord.Core.Models;
using OhMyWord.Core.Services;
using OhMyWord.Integrations.Errors;
using OhMyWord.Integrations.RapidApi.Models.IpGeoLocation;
using OhMyWord.Integrations.RapidApi.Services;
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
    private readonly IGeoLocationApiClient apiClient;

    public GeoLocationService(IGeoLocationRepository repository, IGeoLocationApiClient apiClient)
    {
        this.repository = repository;
        this.apiClient = apiClient;
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
        var apiResponse = await apiClient.GetGeoLocationAsync(ipAddress, cancellationToken);
        geoLocation = MapToGeoLocation(apiResponse);
        await repository.AddGeoLocationAsync(geoLocation);

        return geoLocation;
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
