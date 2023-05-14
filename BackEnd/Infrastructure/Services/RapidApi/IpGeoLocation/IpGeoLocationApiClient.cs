using Microsoft.Extensions.Logging;
using OhMyWord.Infrastructure.Extensions;
using OhMyWord.Infrastructure.Models;
using OhMyWord.Infrastructure.Models.IpGeoLocation;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OhMyWord.Infrastructure.Services.RapidApi.IpGeoLocation;

public interface IIpGeoLocationApiClient
{
    Task<IpGeoLocationEntity> GetIpAddressInfoAsync(string ipAddress, CancellationToken cancellationToken = default);
    Task<IpGeoLocationEntity> GetIpAddressInfoAsync(IPAddress ipAddress, CancellationToken cancellationToken = default);
}

public class IpGeoLocationApiClient : IIpGeoLocationApiClient
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

    public async Task<IpGeoLocationEntity> GetIpAddressInfoAsync(string ipAddress,
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

    public Task<IpGeoLocationEntity> GetIpAddressInfoAsync(IPAddress ipAddress, CancellationToken cancellationToken)
        => GetIpAddressInfoAsync(ipAddress.ToString(), cancellationToken);
}
