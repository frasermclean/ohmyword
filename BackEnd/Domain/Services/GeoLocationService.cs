using FluentResults;
using OhMyWord.Core.Models;
using OhMyWord.Integrations.Errors;
using OhMyWord.Integrations.Models.Entities;
using OhMyWord.Integrations.Services.RapidApi.IpGeoLocation;
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
        // lookup in table storage
        var entity = await repository.GetGeoLocationAsync(ipAddress, cancellationToken);
        if (entity is not null) return MapToGeoLocation(entity);

        // lookup in API and store in table storage
        entity = await apiClient.GetGeoLocationAsync(ipAddress, cancellationToken);
        await repository.AddGeoLocationAsync(entity);

        return MapToGeoLocation(entity);
    }

    private static GeoLocation MapToGeoLocation(GeoLocationEntity entity) => new()
    {
        IpAddress = IPAddress.TryParse(entity.RowKey, out var ipAddress) ? ipAddress : IPAddress.None,
        CountryCode = entity.CountryCode.ToLower(),
        CountryName = entity.CountryName,
        City = entity.City,
        LastUpdated = entity.Timestamp?.UtcDateTime ?? default
    };
}
