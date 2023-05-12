using Microsoft.Extensions.Options;
using OhMyWord.Infrastructure.Options;

namespace OhMyWord.Infrastructure.Services.RapidApi.IpGeoLocation;

public interface IIpGeoLocationService
{
}

public class IpGeoLocationService : IIpGeoLocationService
{
    private readonly HttpClient httpClient;

    public IpGeoLocationService(HttpClient httpClient)
    {
        this.httpClient = httpClient;
    }
}


