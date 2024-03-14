using FluentResults;
using OhMyWord.Core.Models;
using System.Net;

namespace OhMyWord.Core.Services;

public interface IGeoLocationService
{
    Task<Result<GeoLocation>> GetGeoLocationAsync(string ipAddress, CancellationToken cancellationToken = default);
    Task<Result<GeoLocation>> GetGeoLocationAsync(IPAddress ipAddress, CancellationToken cancellationToken = default);
}
