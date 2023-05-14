using Microsoft.Extensions.Logging;
using OhMyWord.Infrastructure.Extensions;
using OhMyWord.Infrastructure.Models.Entities;
using OhMyWord.Infrastructure.Models.IpGeoLocation;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OhMyWord.Infrastructure.Services.RapidApi.IpGeoLocation;

public interface IGeoLocationApiClient
{
    Task<GeoLocationEntity> GetGetLocationAsync(string ipAddress, CancellationToken cancellationToken = default);
    Task<GeoLocationEntity> GetGetLocationAsync(IPAddress ipAddress, CancellationToken cancellationToken = default);
}

public class GeoLocationApiClient : IGeoLocationApiClient
{
    private readonly ILogger<GeoLocationApiClient> logger;
    private readonly HttpClient httpClient;

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true, Converters = { new JsonStringEnumConverter() }
    };

    public GeoLocationApiClient(ILogger<GeoLocationApiClient> logger, HttpClient httpClient)
    {
        this.logger = logger;
        this.httpClient = httpClient;
    }

    public async Task<GeoLocationEntity> GetGetLocationAsync(string ipAddress,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Getting IP address info for: {IpAddress}", ipAddress);

        var uri = new Uri($"{ipAddress}?filter=city,country", UriKind.Relative);
        var apiResponse =
            await httpClient.GetFromJsonAsync<IpGeoLocationApiResponse>(uri, SerializerOptions, cancellationToken);

        if (apiResponse is null)
            throw new InvalidOperationException("Unable to deserialize IP address info");

        return apiResponse.ToEntity();
    }

    public Task<GeoLocationEntity> GetGetLocationAsync(IPAddress ipAddress, CancellationToken cancellationToken)
        => GetGetLocationAsync(ipAddress.ToString(), cancellationToken);
}
