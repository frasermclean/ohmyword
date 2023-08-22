using Microsoft.Extensions.Logging;
using OhMyWord.Core.Models;
using OhMyWord.Core.Services;
using OhMyWord.Integrations.RapidApi.Models.IpGeoLocation;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OhMyWord.Integrations.RapidApi.Services;

/// <summary>
/// IP Geo Location API client service.
/// https://rapidapi.com/natkapral/api/ip-geo-location/
/// </summary>
public class IpGeoLocationApiClient : IGeoLocationClient
{
    private readonly ILogger<IpGeoLocationApiClient> logger;
    private readonly HttpClient httpClient;

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true, Converters = { new JsonStringEnumConverter() }
    };

    public IpGeoLocationApiClient(ILogger<IpGeoLocationApiClient> logger, HttpClient httpClient)
    {
        this.logger = logger;
        this.httpClient = httpClient;
    }

    public async Task<GeoLocation> GetGeoLocationAsync(string ipAddress,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Getting IP address info for: {IpAddress}", ipAddress);

        var uri = new Uri($"{ipAddress}?filter=city,country", UriKind.Relative);
        var apiResponse = await httpClient.GetFromJsonAsync<ApiResponse>(uri, SerializerOptions, cancellationToken);

        if (apiResponse is null)
            throw new InvalidOperationException("Unable to deserialize IP address info");

        return MapToGeoLocation(apiResponse);
    }

    public Task<GeoLocation> GetGeoLocationAsync(IPAddress ipAddress, CancellationToken cancellationToken)
        => GetGeoLocationAsync(ipAddress.ToString(), cancellationToken);

    private static GeoLocation MapToGeoLocation(ApiResponse apiResponse) => new()
    {
        IpAddress = IPAddress.TryParse(apiResponse.IpAddress, out var ipAddress) ? ipAddress : IPAddress.None,
        CountryName = apiResponse.Country.Name ?? string.Empty,
        City = apiResponse.City.Name ?? string.Empty,
        CountryCode = apiResponse.Country.Code ?? string.Empty,
        LastUpdated = DateTime.UtcNow
    };
}
