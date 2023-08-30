using FluentResults;
using OhMyWord.Core.Models;
using OhMyWord.Domain.Errors;
using OhMyWord.Integrations.Models.Entities;
using OhMyWord.Integrations.RapidApi.Models.IpGeoLocation;
using OhMyWord.Integrations.RapidApi.Services;
using OhMyWord.Integrations.Services.Repositories;
using System.Net;

namespace OhMyWord.Domain.Services;

public interface IGeoLocationService
{
    Task<Result<GeoLocation>> GetGeoLocationAsync(string ipAddress, CancellationToken cancellationToken = default);
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
        // lookup geo location from table storage
        var entity = await repository.GetGeoLocationAsync(ipAddress, cancellationToken);
        if (entity is not null)
            return MapToGeoLocation(entity);

        // lookup geo location from API
        var apiResponse = await apiClient.GetGeoLocationAsync(ipAddress, cancellationToken);
        if (apiResponse is null)
            return new IpAddressNotFoundError(ipAddress.ToString());

        // save to table storage
        entity = MapToGeoLocationEntity(apiResponse);
        await repository.AddGeoLocationAsync(entity);

        return MapToGeoLocation(entity);
    }

    private static GeoLocation MapToGeoLocation(GeoLocationEntity entity) => new()
    {
        IpAddress = IPAddress.TryParse(entity.RowKey, out var ipAddress) ? ipAddress : IPAddress.None,
        CountryCode = entity.CountryCode.ToLower(),
        CountryName = entity.CountryName,
        City = entity.City,
    };

    private static GeoLocationEntity MapToGeoLocationEntity(GeoLocationApiResponse apiResponse) => new()
    {
        PartitionKey = apiResponse.IpVersion,
        RowKey = apiResponse.IpAddress,
        CountryName = apiResponse.Country.Name ?? string.Empty,
        City = apiResponse.City.Name ?? string.Empty,
        CountryCode = apiResponse.Country.Code ?? string.Empty
    };
}
