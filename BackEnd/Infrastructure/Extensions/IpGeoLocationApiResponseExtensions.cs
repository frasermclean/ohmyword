using OhMyWord.Infrastructure.Models.Entities;
using OhMyWord.Infrastructure.Models.IpGeoLocation;

namespace OhMyWord.Infrastructure.Extensions;

internal static class IpGeoLocationApiResponseExtensions
{
    internal static GeoLocationEntity ToEntity(this IpGeoLocationApiResponse response) => new()
    {
        PartitionKey = response.IpVersion,
        RowKey = response.IpAddress,
        CountryName = response.Country.Name ?? string.Empty,
        City = response.City.Name ?? string.Empty,
        CountryCode = response.Country.Code ?? string.Empty
    };
}
