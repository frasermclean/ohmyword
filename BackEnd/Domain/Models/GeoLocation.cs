using System.Net;

namespace OhMyWord.Domain.Models;

public record GeoLocation
{    
    public required IPAddress IpAddress { get; init; }
    public required string CountryCode { get; init; }
    public required string CountryName { get; init; }
    public required string City { get; init; }
    public required DateTime LastUpdated { get; init; }

    public static GeoLocation None => new()
    {        
        IpAddress = IPAddress.None,
        CountryCode = string.Empty,
        CountryName = string.Empty,
        City = string.Empty,
        LastUpdated = default
    };
}
