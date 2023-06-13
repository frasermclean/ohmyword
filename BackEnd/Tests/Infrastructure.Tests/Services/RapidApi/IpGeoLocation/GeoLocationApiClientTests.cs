using Microsoft.Extensions.DependencyInjection;
using OhMyWord.Infrastructure.Services.RapidApi.IpGeoLocation;
using System.Net;

namespace Infrastructure.Tests.Services.RapidApi.IpGeoLocation;

[Trait("Category", "Integration")]
public class GeoLocationApiClientTests : IClassFixture<RapidApiFixture>
{
    private readonly IGeoLocationApiClient geoLocationApiClient;

    public GeoLocationApiClientTests(RapidApiFixture fixture)
    {
        geoLocationApiClient = fixture.ServiceProvider.GetRequiredService<IGeoLocationApiClient>();
    }

    [Theory]
    [InlineData("218.195.228.185", "IPv4")]
    [InlineData("2d4b:b2dd:9da1:1d25:3bcf:5997:708e:1587", "IPv6")]
    public async Task GetIpAddressInfo_WithStringAddress_Should_ReturnExpectedResult(string ipAddress, string expectedVersion)
    {
        // act
        var entity = await geoLocationApiClient.GetGeoLocationAsync(ipAddress);

        // assert
        entity.PartitionKey.Should().Be(expectedVersion);
        entity.RowKey.Should().Be(ipAddress);
        entity.CountryCode.Should().NotBeEmpty();
        entity.CountryName.Should().NotBeEmpty();
        entity.City.Should().NotBeEmpty();
    }

    [Theory]
    [InlineData("122.123.192.157", "IPv4")]
    [InlineData("2f8f:7cd7:e9b8:e635:a46f:f329:2156:f06b", "IPv6")]
    public async Task GetIpAddressInfo_WithIPAddressAddress_Should_ReturnExpectedResult(string ipAddress,
        string expectedVersion)
    {
        // act
        var entity = await geoLocationApiClient.GetGeoLocationAsync(IPAddress.Parse(ipAddress));

        // assert
        entity.PartitionKey.Should().Be(expectedVersion);
        entity.RowKey.Should().Be(ipAddress);
        entity.CountryCode.Should().NotBeEmpty();
        entity.CountryName.Should().NotBeEmpty();
        entity.City.Should().NotBeEmpty();
    }
}
