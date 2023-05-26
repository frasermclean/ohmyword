using OhMyWord.Infrastructure.Models.IpGeoLocation;

namespace OhMyWord.Domain.Models;

public record GeoLocation
{
    public required IpVersion IpVersion { get; init; }
    public required string IpAddress { get; init; }
    public required string CountryCode{ get; init; }
    public required string CountryName { get; init; }
    public required string City { get; init; }
    

    public static GeoLocation None => new()
    {
        IpVersion = IpVersion.Invalid,
        IpAddress = string.Empty,
        CountryCode = string.Empty,
        CountryName = string.Empty,
        City = string.Empty,        
    };
}
