using OhMyWord.Core.Models;
using OhMyWord.Core.Services;
using OhMyWord.Domain.Errors;
using OhMyWord.Domain.Services;
using OhMyWord.Integrations.Models.Entities;
using OhMyWord.Integrations.RapidApi.Models.IpGeoLocation;
using OhMyWord.Integrations.RapidApi.Services;
using OhMyWord.Integrations.Services.Repositories;
using OhMyWord.Integrations.Storage.Models;
using OhMyWord.Integrations.Storage.Services;
using System.Net;

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

    [Theory, AutoData]
    public async Task GetGeoLocationAsync_WhenEntityIsInStorage_Should_Return_ExpectedResult(IPAddress ipAddress,
        GeoLocationEntity entity)
    {
        // arrange
        entity.RowKey = ipAddress.ToString();
        SetupGeoLocationRepositoryMock(ipAddress, entity);

        // act
        var result = await geoLocationService.GetGeoLocationAsync(ipAddress);

        // assert        
        result.Should().BeSuccess().Which.Value.Should().BeEquivalentTo(new GeoLocation
        {
            IpAddress = ipAddress,
            CountryCode = entity.CountryCode.ToLower(),
            CountryName = entity.CountryName,
            City = entity.City
        });
        geoLocationRepositoryMock.Verify(
            repository => repository.GetGeoLocationAsync(ipAddress, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetGeoLocationAsync_WhenEntityIsNotInStorage_Should_Return_ExpectedResult()
    {
        // arrange
        var fixture = new Fixture();
        var ipAddress = fixture.Create<IPAddress>();
        var apiResponse = fixture.Build<GeoLocationApiResponse>()
            .With(response => response.IpAddress, ipAddress.ToString())
            .Create();
        SetupGeoLocationRepositoryMock(ipAddress, null);
        SetupGeoLocationApiClientMock(ipAddress, apiResponse);

        // act
        var result = await geoLocationService.GetGeoLocationAsync(ipAddress);

        // assert
        result.Should().BeSuccess().Which.Value.Should().BeEquivalentTo(new GeoLocation
        {
            IpAddress = ipAddress,
            CountryCode = apiResponse.Country.Code!.ToLower(),
            CountryName = apiResponse.Country.Name!,
            City = apiResponse.City.Name!
        });
        geoLocationRepositoryMock.Verify(
            repository => repository.GetGeoLocationAsync(ipAddress, It.IsAny<CancellationToken>()), Times.Once);
        geoLocationApiClientMock.Verify(client => client.GetGeoLocationAsync(ipAddress, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Theory, AutoData]
    public async Task GetGeoLocationAsync_WhenIpAddressCanNotBeFound_Should_Return_Error(IPAddress ipAddress)
    {
        // arrange
        SetupGeoLocationRepositoryMock(ipAddress, null);
        SetupGeoLocationApiClientMock(ipAddress, null);

        // act
        var result = await geoLocationService.GetGeoLocationAsync(ipAddress);

        // assert
        result.Should().BeFailure().Which.Should()
            .HaveReason<IpAddressNotFoundError>($"IP address not found: {ipAddress}");
        geoLocationRepositoryMock.Verify(
            repository => repository.GetGeoLocationAsync(ipAddress, It.IsAny<CancellationToken>()), Times.Once);
        geoLocationApiClientMock.Verify(client => client.GetGeoLocationAsync(ipAddress, It.IsAny<CancellationToken>()),
            Times.Once);
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

    private void SetupGeoLocationRepositoryMock(IPAddress ipAddress, GeoLocationEntity? entityToReturn)
    {
        geoLocationRepositoryMock.Setup(repository =>
                repository.GetGeoLocationAsync(ipAddress, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entityToReturn);
    }

    private void SetupGeoLocationApiClientMock(IPAddress ipAddress, GeoLocationApiResponse? apiResponseToReturn)
    {
        geoLocationApiClientMock.Setup(apiClient =>
                apiClient.GetGeoLocationAsync(ipAddress, It.IsAny<CancellationToken>()))
            .ReturnsAsync(apiResponseToReturn);
    }
}
