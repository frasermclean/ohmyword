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

    [Theory]
    [InlineData("218.195.228.185")]
    [InlineData("2d4b:b2dd:9da1:1d25:3bcf:5997:708e:1587")]
    public async Task GetGeoLocationAsync_Should_Return_ExpectedResult(string ipAddress)
    {
        // act
        var location = await geoLocationService.GetGeoLocationAsync(ipAddress);

        // assert
        location.IpAddress.Should().Be(ipAddress);
        location.CountryCode.Should().NotBeEmpty();
        location.CountryName.Should().NotBeEmpty();
        location.City.Should().NotBeEmpty();
    }
}
