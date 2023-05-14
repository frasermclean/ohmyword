using OhMyWord.Infrastructure.Models.Entities;
using OhMyWord.Infrastructure.Models.IpGeoLocation;

namespace OhMyWord.Infrastructure.Extensions;

internal static class IpGeoLocationApiResponseExtensions
{
    internal static GeoLocationEntity ToEntity(this IpGeoLocationApiResponse response) => new()
    {
        PartitionKey = response.IpVersion.ToString(),
        RowKey = response.IpAddress,
        Country = response.Country.Name ?? string.Empty,
        City = response.City.Name ?? string.Empty,
        FlagUrl = response.Country.Flag.Url ?? string.Empty
    };
}
