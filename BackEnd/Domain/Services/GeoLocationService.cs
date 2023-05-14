using OhMyWord.Infrastructure.Services;
using OhMyWord.Infrastructure.Services.RapidApi.IpGeoLocation;

namespace OhMyWord.Domain.Services;

public interface IGeoLocationService
{
}

public class GeoLocationService : IGeoLocationService
{
    private readonly IGeoLocationRepository repository;
    private readonly IGeoLocationApiClient apiClient;

    public GeoLocationService(IGeoLocationRepository repository, IGeoLocationApiClient apiClient)
    {
        this.repository = repository;
        this.apiClient = apiClient;
    }
}
