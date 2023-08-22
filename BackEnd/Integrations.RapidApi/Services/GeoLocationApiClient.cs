using Microsoft.Extensions.Logging;
using OhMyWord.Integrations.RapidApi.Models.IpGeoLocation;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OhMyWord.Integrations.RapidApi.Services;

public interface IGeoLocationApiClient
{
    Task<ApiResponse> GetGeoLocationAsync(string ipAddress, CancellationToken cancellationToken = default);

    Task<ApiResponse> GetGeoLocationAsync(IPAddress ipAddress, CancellationToken cancellationToken = default);
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

    public async Task<ApiResponse> GetGeoLocationAsync(string ipAddress,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Getting IP address info for: {IpAddress}", ipAddress);

        var uri = new Uri($"{ipAddress}?filter=city,country", UriKind.Relative);
        var response = await httpClient.GetFromJsonAsync<ApiResponse>(uri, SerializerOptions, cancellationToken);

        return response ?? throw new InvalidOperationException("Unable to deserialize IP address info");
    }

    public Task<ApiResponse> GetGeoLocationAsync(IPAddress ipAddress, CancellationToken cancellationToken)
        => GetGeoLocationAsync(ipAddress.ToString(), cancellationToken);
}
