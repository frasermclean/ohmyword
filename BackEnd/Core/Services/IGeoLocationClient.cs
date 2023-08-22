using OhMyWord.Core.Models;
using System.Net;

namespace OhMyWord.Core.Services;

/// <summary>
/// Client service responsible for looking up the geolocation of an IP address from an external API.
/// </summary>
public interface IGeoLocationClient
{
    Task<GeoLocation> GetGeoLocationAsync(string ipAddress, CancellationToken cancellationToken = default);

    Task<GeoLocation> GetGeoLocationAsync(IPAddress ipAddress, CancellationToken cancellationToken = default);
}
