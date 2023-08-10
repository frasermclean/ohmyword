using NSubstitute.ReturnsExtensions;
using OhMyWord.Domain.Services;
using OhMyWord.Integrations.Errors;
using OhMyWord.Integrations.Models.Entities;
using OhMyWord.Integrations.Services.RapidApi.IpGeoLocation;
using OhMyWord.Integrations.Services.Repositories;
using System.Net;
using System.Net.Sockets;

namespace OhMyWord.Domain.Tests.Services;

[Trait("Category", "Unit")]
public class GeoLocationServiceTests
{
    private readonly IGeoLocationService geoLocationService;
    private readonly IGeoLocationRepository geoLocationRepository = Substitute.For<IGeoLocationRepository>();
    private readonly IGeoLocationApiClient geoLocationApiClient = Substitute.For<IGeoLocationApiClient>();

    public GeoLocationServiceTests()
    {
        geoLocationService = new GeoLocationService(geoLocationRepository, geoLocationApiClient);
    }

    [Theory]
    [InlineData("218.195.228.185")]
    [InlineData("2d4b:b2dd:9da1:1d25:3bcf:5997:708e:1587")]
    public async Task GetGeoLocationAsync_WhenLocationIsInRepository_Should_Return_ExpectedResult(string address)
    {
        // arrange
        var entity = new GeoLocationEntity
        {
            RowKey = address, CountryCode = "AU", CountryName = "Australia", City = "Melbourne"
        };

        geoLocationRepository.GetGeoLocationAsync(Arg.Any<IPAddress>(), Arg.Any<CancellationToken>())
            .Returns(info => info.Arg<IPAddress>().AddressFamily == AddressFamily.InterNetworkV6
                ? entity with { PartitionKey = "IPv6" }
                : entity with { PartitionKey = "IPv4" });

        // act
        var result = await geoLocationService.GetGeoLocationAsync(address);

        // assert        
        result.Should().BeSuccess();
        result.Value.IpAddress.ToString().Should().Be(address);
        result.Value.CountryCode.Should().Be("au");
        result.Value.CountryName.Should().Be("Australia");
        result.Value.City.Should().Be("Melbourne");
    }

    [Theory]
    [InlineData("218.195.228.185")]
    [InlineData("2d4b:b2dd:9da1:1d25:3bcf:5997:708e:1587")]
    public async Task GetGeoLocationAsync_WhenLocationIsNotInRepository_Should_Return_ExpectedResult(string address)
    {
        // arrange
        var entity = new GeoLocationEntity
        {
            RowKey = address, CountryCode = "AU", CountryName = "Australia", City = "Melbourne"
        };

        geoLocationRepository.GetGeoLocationAsync(Arg.Any<IPAddress>(), Arg.Any<CancellationToken>())
            .ReturnsNull();
        geoLocationApiClient.GetGeoLocationAsync(Arg.Any<IPAddress>(), Arg.Any<CancellationToken>())
            .Returns(info => info.Arg<IPAddress>().AddressFamily == AddressFamily.InterNetworkV6
                ? entity with { PartitionKey = "IPv6" }
                : entity with { PartitionKey = "IPv4" });

        // act
        var result = await geoLocationService.GetGeoLocationAsync(address);

        // assert
        result.Should().BeSuccess();
        result.Value.IpAddress.ToString().Should().Be(address);
        result.Value.CountryCode.Should().Be("au");
        result.Value.CountryName.Should().Be("Australia");
        result.Value.City.Should().Be("Melbourne");
    }

    [Fact]
    public async Task GetGeoLocationAsync_Should_Return_Error_When_Address_Is_Invalid()
    {
        // arrange
        const string address = "invalid";

        // act
        var result = await geoLocationService.GetGeoLocationAsync(address);

        // assert
        result.Should().BeFailure().Which
            .Should().HaveReason<InvalidIpAddressError>($"Invalid IP address: {address}");
    }
}
