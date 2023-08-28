using OhMyWord.Core.Services;
using OhMyWord.Domain.Services;
using OhMyWord.Integrations.Errors;
using OhMyWord.Integrations.RapidApi.Services;
using OhMyWord.Logic.Services;
using System.Net;
using System.Net.Sockets;

namespace OhMyWord.Domain.Tests.Services;

[Trait("Category", "Unit")]
public class GeoLocationServiceTests
{
    private readonly IGeoLocationService geoLocationService;
    private readonly Mock<IGeoLocationRepository> geoLocationRepositoryMock = new();
    private readonly Mock<IGeoLocationApiClient> geoLocationApiClientMock = new();

    public GeoLocationServiceTests()
    {
        geoLocationService = new GeoLocationService(geoLocationRepositoryMock.Object, geoLocationApiClientMock.Object);
    }

    [Theory]
    [InlineData("218.195.228.185")]
    [InlineData("2d4b:b2dd:9da1:1d25:3bcf:5997:708e:1587")]
    public async Task GetGeoLocationAsync_Should_Return_ExpectedResult(string address)
    {
        // arrange
        var entity = new GeoLocationEntity { CountryCode = "AU", CountryName = "Australia", City = "Melbourne" };

        geoLocationRepositoryMock.Setup(repository =>
                repository.GetGeoLocationAsync(It.IsAny<IPAddress>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IPAddress ipAddress, CancellationToken _) =>
                ipAddress.AddressFamily == AddressFamily.InterNetwork
                    ? entity with { PartitionKey = "IPv4", RowKey = ipAddress.ToString() }
                    : null);

        geoLocationApiClientMock.Setup(apiClient =>
                apiClient.GetGeoLocationAsync(It.IsAny<IPAddress>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IPAddress ipAddress, CancellationToken _) =>
                entity with { PartitionKey = "IPv6", RowKey = ipAddress.ToString() });

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
