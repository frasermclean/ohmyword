﻿using Microsoft.Extensions.Logging;
using OhMyWord.Infrastructure.Models.IpGeoLocation;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OhMyWord.Infrastructure.Services.RapidApi.IpGeoLocation;

public interface IIpGeoLocationService
{
    Task<IpGeoLocationData> GetIpAddressInfoAsync(string ipAddress, CancellationToken cancellationToken = default);
    Task<IpGeoLocationData> GetIpAddressInfoAsync(IPAddress ipAddress, CancellationToken cancellationToken = default);
}

public class IpGeoLocationService : IIpGeoLocationService
{
    private readonly ILogger<IpGeoLocationService> logger;
    private readonly HttpClient httpClient;

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true, Converters = { new JsonStringEnumConverter() }
    };

    public IpGeoLocationService(ILogger<IpGeoLocationService> logger, HttpClient httpClient)
    {
        this.logger = logger;
        this.httpClient = httpClient;
    }

    public async Task<IpGeoLocationData> GetIpAddressInfoAsync(string ipAddress,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Getting IP address info for: {IpAddress}", ipAddress);

        var uri = new Uri($"{ipAddress}?filter=city,country", UriKind.Relative);
        var ipAddressInfo =
            await httpClient.GetFromJsonAsync<IpGeoLocationData>(uri, SerializerOptions, cancellationToken);

        return ipAddressInfo ?? throw new InvalidOperationException("Unable to deserialize IP address info");
    }

    public Task<IpGeoLocationData> GetIpAddressInfoAsync(IPAddress ipAddress, CancellationToken cancellationToken)
        => GetIpAddressInfoAsync(ipAddress.ToString(), cancellationToken);
}
