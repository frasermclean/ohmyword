using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OhMyWord.Domain.Extensions;
using OhMyWord.Domain.Models;
using OhMyWord.Domain.Options;
using OhMyWord.Domain.Services;
using OhMyWord.Infrastructure.Models.Entities;
using OhMyWord.Infrastructure.Services;

namespace Domain.Tests.Services;

public sealed class RoundServiceTests
{
    private readonly IRoundService roundService;
    private readonly Mock<IWordsService> wordsServiceMock = new();
    private readonly Mock<IRoundsRepository> roundsRepositoryMock = new();

    public RoundServiceTests()
    {
        var playerServiceMock = new Mock<IPlayerService>();
        playerServiceMock.Setup(service => service.PlayerIds)
            .Returns(new[] { "abc123", "def456" });

        var options = new RoundOptions { LetterHintDelay = 3, PostRoundDelay = 5, GuessLimit = 3 };

        roundService = new RoundService(Options.Create(options), Mock.Of<ILogger<RoundService>>(),
            playerServiceMock.Object, wordsServiceMock.Object, roundsRepositoryMock.Object);
    }

    [Theory, AutoData]
    public async Task CreateRoundAsync_Should_ReturnExpectedResults(int roundNumber, Word word, Guid sessionId)
    {
        // arrange
        wordsServiceMock
            .Setup(service => service.GetRandomWordAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(word);

        // act
        using var round = await roundService.CreateRoundAsync(roundNumber, sessionId);

        // assert
        round.Id.Should().NotBeEmpty();
        round.Number.Should().Be(roundNumber);
        round.Word.Should().Be(word);
        round.GuessLimit.Should().Be(3);
        round.WordHint.Length.Should().Be(word.Id.Length);
        round.WordHint.LetterHints.Should().BeEmpty();
        round.StartDate.Should().BeBefore(DateTime.UtcNow);
        round.EndDate.Should().BeAfter(DateTime.UtcNow);
        round.PlayerCount.Should().Be(2);
        round.SessionId.Should().Be(sessionId);
    }

    [Theory, AutoData]
    public async Task SaveRoundAsync_Should_ReturnExpectedResult(Round round)
    {
        // arrange
        var entity = round.ToEntity();
        roundsRepositoryMock
            .Setup(repository => repository.CreateRoundAsync(It.IsAny<RoundEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);
        
        // act
        await roundService.SaveRoundAsync(round);
    }
}
