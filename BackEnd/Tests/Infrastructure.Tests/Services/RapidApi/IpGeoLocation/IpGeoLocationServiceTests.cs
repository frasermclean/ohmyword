using Microsoft.Extensions.DependencyInjection;
using OhMyWord.Infrastructure.Models.IpGeoLocation;
using OhMyWord.Infrastructure.Services.RapidApi.IpGeoLocation;
using System.Net;

namespace Infrastructure.Tests.Services.RapidApi.IpGeoLocation;

public class IpGeoLocationServiceTests : IClassFixture<RapidApiFixture>
{
    private readonly IIpGeoLocationService ipGeoLocationService;

    public IpGeoLocationServiceTests(RapidApiFixture fixture)
    {
        ipGeoLocationService = fixture.ServiceProvider.GetRequiredService<IIpGeoLocationService>();
    }

    [Theory]
    [InlineData("127.0.0.1", IpVersion.Ipv4)]
    [InlineData("::1", IpVersion.Ipv6)]
    public async Task GetIpAddressInfo_WithStringAddress_Should_ReturnExpectedResult(string ipAddress,
        IpVersion expectedVersion)
    {
        // act
        var data = await ipGeoLocationService.GetIpAddressInfoAsync(ipAddress);

        // assert
        data.IpAddress.Should().Be(ipAddress);
        data.IpVersion.Should().Be(expectedVersion);
    }
    
    [Theory]
    [InlineData("127.0.0.1", IpVersion.Ipv4)]
    [InlineData("::1", IpVersion.Ipv6)]
    public async Task GetIpAddressInfo_WithIPAddressAddress_Should_ReturnExpectedResult(string ipAddress,
        IpVersion expectedVersion)
    {        
        // act
        var data = await ipGeoLocationService.GetIpAddressInfoAsync(IPAddress.Parse(ipAddress));

        // assert
        data.IpAddress.Should().Be(ipAddress);
        data.IpVersion.Should().Be(expectedVersion);
    }
}
