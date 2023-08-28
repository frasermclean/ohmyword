using OhMyWord.Core.Services;
using OhMyWord.Integrations.RapidApi.Tests.Fixtures;
using System.Net;

namespace OhMyWord.Integrations.RapidApi.Tests.Services;

[Trait("Category", "Integration")]
public class GeoLocationApiClientTests : IClassFixture<RapidApiFixture>
{
    private readonly IGeoLocationClient geoLocationApiClient;

    public GeoLocationApiClientTests(RapidApiFixture fixture)
    {
        geoLocationApiClient = fixture.GeoLocationApiClient;
    }

    [Theory]
    [InlineData("218.195.228.185")]
    [InlineData("2d4b:b2dd:9da1:1d25:3bcf:5997:708e:1587")]
    public async Task GetIpAddressInfo_WithStringAddress_Should_ReturnExpectedResult(string ipAddress)
    {
        // act
        var geoLocation = await geoLocationApiClient.GetGeoLocationAsync(ipAddress);

        // assert
        geoLocation.Should().NotBeNull();
        geoLocation.IpAddress.Should().Be(IPAddress.Parse(ipAddress));
    }

    [Theory]
    [InlineData("122.123.192.157")]
    [InlineData("2f8f:7cd7:e9b8:e635:a46f:f329:2156:f06b")]
    public async Task GetIpAddressInfo_WithIPAddressAddress_Should_ReturnExpectedResult(string ipAddress)
    {
        // act
        var geoLocation = await geoLocationApiClient.GetGeoLocationAsync(IPAddress.Parse(ipAddress));

        // assert
        geoLocation.Should().NotBeNull();
        geoLocation.IpAddress.Should().Be(IPAddress.Parse(ipAddress));
    }
}
