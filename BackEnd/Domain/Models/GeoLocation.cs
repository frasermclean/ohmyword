using OhMyWord.Infrastructure.Models.IpGeoLocation;

namespace OhMyWord.Domain.Models;

public record GeoLocation
{
    public required IpVersion IpVersion { get; init; }
    public required string IpAddress { get; init; }
    public required string Country { get; init; }
    public required string City { get; init; }
    public required string FlagUrl { get; init; }

    public static GeoLocation None => new()
    {
        IpVersion = IpVersion.Invalid,
        IpAddress = string.Empty,
        Country = string.Empty,
        City = string.Empty,
        FlagUrl = string.Empty
    };
}
