using Microsoft.Extensions.DependencyInjection;
using OhMyWord.Infrastructure.Models.IpGeoLocation;
using OhMyWord.Infrastructure.Services.RapidApi.IpGeoLocation;
using System.Net;

namespace Infrastructure.Tests.Services.RapidApi.IpGeoLocation;

[Trait("Category", "Integration")]
public class IpGeoLocationServiceTests : IClassFixture<RapidApiFixture>
{
    private readonly IIpGeoLocationService ipGeoLocationService;

    public IpGeoLocationServiceTests(RapidApiFixture fixture)
    {
        ipGeoLocationService = fixture.ServiceProvider.GetRequiredService<IIpGeoLocationService>();
    }

    [Theory]
    [InlineData("218.195.228.185", IpVersion.Ipv4)]
    [InlineData("2d4b:b2dd:9da1:1d25:3bcf:5997:708e:1587", IpVersion.Ipv6)]
    public async Task GetIpAddressInfo_WithStringAddress_Should_ReturnExpectedResult(string ipAddress,
        IpVersion expectedVersion)
    {
        // act
        var data = await ipGeoLocationService.GetIpAddressInfoAsync(ipAddress);

        // assert
        data.IpAddress.Should().Be(ipAddress);
        data.IpVersion.Should().Be(expectedVersion);
        data.City.Name.Should().NotBeEmpty();
        data.Country.Name.Should().NotBeEmpty();
        data.Country.Code.Should().NotBeEmpty();
        data.Country.Flag.Url.Should().NotBeEmpty();
    }

    [Theory]
    [InlineData("122.123.192.157", IpVersion.Ipv4)]
    [InlineData("2f8f:7cd7:e9b8:e635:a46f:f329:2156:f06b", IpVersion.Ipv6)]
    public async Task GetIpAddressInfo_WithIPAddressAddress_Should_ReturnExpectedResult(string ipAddress,
        IpVersion expectedVersion)
    {
        // act
        var data = await ipGeoLocationService.GetIpAddressInfoAsync(IPAddress.Parse(ipAddress));

        // assert
        data.IpAddress.Should().Be(ipAddress);
        data.IpVersion.Should().Be(expectedVersion);
        data.City.Name.Should().NotBeEmpty();
        data.Country.Name.Should().NotBeEmpty();
        data.Country.Code.Should().NotBeEmpty();
        data.Country.Flag.Url.Should().NotBeEmpty();
    }
}
