using OhMyWord.Core.Models;
using System.Net;

namespace OhMyWord.Core.Services;

public interface IGeoLocationRepository
{
    Task<GeoLocation?> GetGeoLocationAsync(string ipAddress, CancellationToken cancellationToken = default);
    Task<GeoLocation?> GetGeoLocationAsync(IPAddress ipAddress, CancellationToken cancellationToken = default);
    Task AddGeoLocationAsync(GeoLocation geoLocation);
}
