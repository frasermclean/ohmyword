using Microsoft.Extensions.Logging;
using OhMyWord.Core.Models;
using OhMyWord.Domain.Services;
using OhMyWord.Domain.Services.State;
using OhMyWord.Integrations.Services.Repositories;

namespace OhMyWord.Domain.Tests.Services;

public class RoundServiceTests : IClassFixture<TestDataFixture>
{
    private readonly TestDataFixture fixture;
    private readonly IRoundService roundService;
    private readonly Mock<IPlayerState> playerStateMock = new();
    private readonly Mock<IWordQueueService> wordQueueServiceMock = new();
    private readonly Mock<IRoundsRepository> roundsRepositoryMock = new();
    private readonly Mock<IPlayerService> playerServiceMock = new();

    public RoundServiceTests(TestDataFixture fixture)
    {
        this.fixture = fixture;

        roundService = new RoundService(Mock.Of<ILogger<RoundService>>(), fixture.CreateOptions(),
            playerStateMock.Object, wordQueueServiceMock.Object, roundsRepositoryMock.Object, playerServiceMock.Object);
    }

    [Fact]
    public async Task CreateRoundAsync_Should_Return_ExpectedValues()
    {
        // arrange
        var word = fixture.CreateWord();
        wordQueueServiceMock.Setup(service => service.GetNextWordAsync(false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(word);

        // act
        var round = await roundService.CreateRoundAsync(1, Guid.NewGuid());

        // assert
        round.Id.Should().NotBeEmpty();
        round.Number.Should().Be(1);
        round.Word.Should().Be(word);
        round.WordHint.Should().NotBeNull();
    }

    [Fact]
    public async Task ExecuteRoundAsync_Should_Return_ExpectedValues()
    {
        // arrange
        playerStateMock.Setup(state => state.GetPlayerById(It.IsAny<Guid>()))
            .Returns<Guid>(TestDataFixture.CreatePlayer);
        var round = fixture.CreateRound();

        // act
        var summary = await roundService.ExecuteRoundAsync(round);

        // assert
        summary.Word.Should().Be(round.Word.Id);
        summary.PartOfSpeech.Should().Be(round.WordHint.PartOfSpeech);
        summary.RoundId.Should().Be(round.Id);
        summary.DefinitionId.Should().NotBeEmpty();
        summary.EndReason.Should().Be(RoundEndReason.AllPlayersGuessed);
        summary.NextRoundStart.Should().BeAfter(DateTime.UtcNow);
        summary.Scores.Should().HaveCount(3);
        summary.Scores.Should().AllSatisfy(line =>
        {
            line.PlayerName.Should().NotBeEmpty();
            line.PointsAwarded.Should().Be(100);
            line.GuessCount.Should().Be(1);
            line.ConnectionId.Should().NotBeEmpty();
            line.CountryName.Should().NotBeEmpty();
            line.CountryCode.Should().NotBeEmpty();
            line.GuessTimeMilliseconds.Should().BePositive();
        });
    }
}
