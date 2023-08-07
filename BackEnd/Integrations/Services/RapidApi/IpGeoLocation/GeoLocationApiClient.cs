﻿using Microsoft.Extensions.Logging;
using OhMyWord.Integrations.Extensions;
using OhMyWord.Integrations.Models.Entities;
using OhMyWord.Integrations.Models.IpGeoLocation;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OhMyWord.Integrations.Services.RapidApi.IpGeoLocation;

public interface IGeoLocationApiClient
{
    Task<GeoLocationEntity> GetGeoLocationAsync(string ipAddress, CancellationToken cancellationToken = default);
    Task<GeoLocationEntity> GetGeoLocationAsync(IPAddress ipAddress, CancellationToken cancellationToken = default);
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

    public async Task<GeoLocationEntity> GetGeoLocationAsync(string ipAddress,
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

    public Task<GeoLocationEntity> GetGeoLocationAsync(IPAddress ipAddress, CancellationToken cancellationToken)
        => GetGeoLocationAsync(ipAddress.ToString(), cancellationToken);
}
