using OhMyWord.Domain.Models;
using OhMyWord.Infrastructure.Models.Entities;
using OhMyWord.Infrastructure.Models.IpGeoLocation;

namespace OhMyWord.Domain.Extensions;

public static class GeoLocationEntityExtensions
{
    public static GeoLocation ToGeoLocation(this GeoLocationEntity entity) => new()
    {
        IpVersion = entity.PartitionKey == "IPv6" ? IpVersion.IPv6 : IpVersion.IPv4, 
        IpAddress = entity.RowKey,
        CountryCode = entity.CountryCode,
        CountryName = entity.CountryName,
        City = entity.City,
    };
}
