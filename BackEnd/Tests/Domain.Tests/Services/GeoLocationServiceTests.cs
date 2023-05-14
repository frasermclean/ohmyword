using OhMyWord.Domain.Services;

namespace Domain.Tests.Services;

[Trait("Category", "Integration")]
public class GeoLocationServiceTests : IClassFixture<ServicesFixture>
{
    private readonly IGeoLocationService geoLocationService;

    public GeoLocationServiceTests(ServicesFixture fixture)
    {
        geoLocationService = fixture.GeoLocationService;
    }

    [Fact]
    public void Test()
    {        
    }
}
