using MediatR;
using Microsoft.Extensions.Logging;
using OhMyWord.Domain.Services;
using OhMyWord.Infrastructure.Models.Entities;
using OhMyWord.Infrastructure.Services;
using System.Net;

namespace Domain.Tests.Services;

public class RoundServiceTests : IClassFixture<TestDataFixture>
{
    private readonly TestDataFixture fixture;
    private readonly IRoundService roundService;
    private readonly Mock<IPublisher> publisherMock = new();
    private readonly Mock<IPlayerState> playerStateMock = new();
    private readonly Mock<IWordsService> wordsServiceMock = new();
    private readonly Mock<IRoundsRepository> roundsRepositoryMock = new();
    private readonly Mock<IGeoLocationService> geoLocationServiceMock = new();

    public RoundServiceTests(TestDataFixture fixture)
    {
        this.fixture = fixture;

        roundService = new RoundService(Mock.Of<ILogger<RoundService>>(), fixture.CreateOptions(), publisherMock.Object,
            playerStateMock.Object, wordsServiceMock.Object, roundsRepositoryMock.Object,
            geoLocationServiceMock.Object);
    }

    [Fact]
    public async Task GetRoundEndDataAsync_Should_Return_ExpectedValues()
    {
        // arrange
        playerStateMock.Setup(state => state.GetPlayerById(It.IsAny<Guid>()))
            .Returns<Guid>((playerId) => fixture.CreatePlayer(playerId));
        geoLocationServiceMock.Setup(service =>
                service.GetCountryCodeAsync(It.IsAny<IPAddress>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("AU");
        var round = fixture.CreateRound();

        // act
        var (postRoundDelay, summary) = await roundService.GetRoundEndDataAsync(round);

        // assert
        postRoundDelay.Should().Be(TimeSpan.FromSeconds(5));
        summary.Word.Should().Be(round.Word.Id);
        summary.RoundId.Should().Be(round.Id);
        summary.DefinitionId.Should().NotBeEmpty();
        summary.EndReason.Should().Be(RoundEndReason.AllPlayersGuessed);
        summary.NextRoundStart.Should().BeAfter(DateTime.UtcNow);
        summary.Scores.Should().HaveCount(3);
        summary.Scores.Should().AllSatisfy(line => line.PlayerName.Should().NotBeEmpty());
        summary.Scores.Should().AllSatisfy(line => line.PointsAwarded.Should().Be(100));
        summary.Scores.Should().AllSatisfy(line => line.GuessCount.Should().Be(1));
        summary.Scores.Should().AllSatisfy(line => line.ConnectionId.Should().NotBeEmpty());
        summary.Scores.Should().AllSatisfy(line => line.CountryCode.Should().NotBeEmpty());
        summary.Scores.Should().AllSatisfy(line => line.GuessTimeMilliseconds.Should().BePositive());
    }
}
