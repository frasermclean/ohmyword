using System.Net;

namespace OhMyWord.Core.Models;

public record GeoLocation
{
    public required IPAddress IpAddress { get; init; }
    public required string CountryCode { get; init; }
    public required string CountryName { get; init; }
    public required string City { get; init; }
}
