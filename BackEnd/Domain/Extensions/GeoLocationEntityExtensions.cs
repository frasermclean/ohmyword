using OhMyWord.Domain.Models;
using OhMyWord.Infrastructure.Models.Entities;
using System.Net;

namespace OhMyWord.Domain.Extensions;

public static class GeoLocationEntityExtensions
{
    public static GeoLocation ToGeoLocation(this GeoLocationEntity entity) => new()
    {         
        IpAddress = IPAddress.TryParse(entity.RowKey, out var ipAddress) ? ipAddress : IPAddress.None,
        CountryCode = entity.CountryCode,
        CountryName = entity.CountryName,
        City = entity.City,
        LastUpdated = entity.Timestamp?.UtcDateTime ?? default
    };
}
