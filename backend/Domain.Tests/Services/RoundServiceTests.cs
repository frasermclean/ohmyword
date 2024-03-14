using FastEndpoints;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OhMyWord.Core.Models;
using OhMyWord.Domain.Contracts.Events;
using OhMyWord.Domain.Options;
using OhMyWord.Domain.Services;
using OhMyWord.Domain.Services.State;
using OhMyWord.Integrations.CosmosDb.Services;

namespace OhMyWord.Domain.Tests.Services;

public class RoundServiceTests
{
    private readonly IRoundService roundService;
    private readonly Mock<IPlayerState> playerStateMock = new();
    private readonly Mock<IWordQueueService> wordQueueServiceMock = new();
    private readonly Mock<IRoundsRepository> roundsRepositoryMock = new();
    private readonly Mock<IPlayerService> playerServiceMock = new();
    private readonly Mock<IEventHandler<LetterHintAddedEvent>> letterHintEventHandlerMock = new();

    public RoundServiceTests()
    {
        var options = Microsoft.Extensions.Options.Options.Create(new RoundOptions
        {
            LetterHintDelay = 0.01, PostRoundDelay = 5, GuessLimit = 3
        });

        new DefaultHttpContext().AddTestServices(collection =>
        {
            collection.AddSingleton(letterHintEventHandlerMock.Object);
        });

        roundService = new RoundService(Mock.Of<ILogger<RoundService>>(), options,
            playerStateMock.Object, wordQueueServiceMock.Object, roundsRepositoryMock.Object, playerServiceMock.Object);
    }

    [Theory, AutoData]
    public async Task CreateRoundAsync_WithNoPlayers_Should_Return_ExpectedValues(Word word, int roundNumber,
        Guid sessionId)
    {
        // arrange
        wordQueueServiceMock.Setup(service => service.GetNextWordAsync(false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(word);

        // act
        var round = await roundService.CreateRoundAsync(roundNumber, sessionId);

        // assert
        round.Id.Should().NotBeEmpty();
        round.Number.Should().Be(roundNumber);
        round.Word.Should().Be(word);
        round.WordHint.Length.Should().Be(word.Length);
        word.Definitions.Should().Contain(round.WordHint.Definition);
        round.WordHint.LetterHints.Should().BeEmpty();
        round.GuessLimit.Should().Be(3);
        round.StartDate.Should().BeBefore(DateTime.UtcNow);
        round.EndDate.Should().BeAfter(DateTime.UtcNow);
        round.SessionId.Should().Be(sessionId);
        round.EndReason.Should().BeNull();
        round.PlayerData.Should().BeEmpty();
        round.AllPlayersAwarded.Should().BeFalse();

        wordQueueServiceMock.Verify(service => service.GetNextWordAsync(false, It.IsAny<CancellationToken>()),
            Times.Once);
        playerStateMock.Verify(state => state.PlayerIds, Times.Once);
    }

    [Theory, AutoData]
    public async Task CreateRoundAsync_WithPlayers_Should_Return_ExpectedValues(Word word, int roundNumber,
        Guid sessionId, Guid[] playerIds, RoundEndReason endReason)
    {
        // arrange
        wordQueueServiceMock.Setup(service => service.GetNextWordAsync(false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(word);
        playerStateMock.Setup(state => state.PlayerIds).Returns(playerIds);

        // act
        var round = await roundService.CreateRoundAsync(roundNumber, sessionId);
        round.EndReason = endReason;

        // assert
        round.Id.Should().NotBeEmpty();
        round.Number.Should().Be(roundNumber);
        round.Word.Should().Be(word);
        round.WordHint.Length.Should().Be(word.Length);
        word.Definitions.Should().Contain(round.WordHint.Definition);
        round.WordHint.LetterHints.Should().BeEmpty();
        round.GuessLimit.Should().Be(3);
        round.StartDate.Should().BeBefore(DateTime.UtcNow);
        round.EndDate.Should().BeAfter(DateTime.UtcNow);
        round.SessionId.Should().Be(sessionId);
        round.EndReason.Should().Be(endReason);
        round.PlayerData.Should().HaveCount(playerIds.Length);
        round.PlayerData.Should().AllSatisfy(pair =>
        {
            playerIds.Should().Contain(pair.Key);
            pair.Value.PointsAwarded.Should().Be(0);
            pair.Value.GuessCount.Should().Be(0);
            pair.Value.GuessTime.Should().Be(TimeSpan.Zero);
        });
        round.AllPlayersAwarded.Should().BeFalse();


        wordQueueServiceMock.Verify(service => service.GetNextWordAsync(false, It.IsAny<CancellationToken>()),
            Times.Once);
        playerStateMock.Verify(state => state.PlayerIds, Times.Once);
    }

    [Theory, AutoData]
    public async Task ExecuteRoundAsync_Should_Return_ExpectedValues(Word word, Guid[] playerIds, int roundNumber,
        Guid sessionId)
    {
        // arrange
        SetupWordQueueService(word);
        SetupPlayerState(playerIds);

        // act
        var round = await roundService.CreateRoundAsync(roundNumber, sessionId);
        var summary = await roundService.ExecuteRoundAsync(round);

        // assert
        round.PlayerData.Should().HaveCount(playerIds.Length);
        summary.Word.Should().Be(word.Id);
        summary.PartOfSpeech.Should().Be(round.WordHint.Definition.PartOfSpeech);
        summary.RoundId.Should().Be(round.Id);
        summary.DefinitionId.Should().NotBeEmpty();
        summary.EndReason.Should().Be(RoundEndReason.Timeout);
        summary.NextRoundStart.Should().BeAfter(DateTime.UtcNow);
        summary.Scores.Should().BeEmpty();
        letterHintEventHandlerMock.Verify(
            handler => handler.HandleAsync(It.IsAny<LetterHintAddedEvent>(), It.IsAny<CancellationToken>()),
            Times.Exactly(round.Word.Length));
    }

    private void SetupWordQueueService(Word? word = default)
    {
        word ??= new Fixture().Create<Word>();

        wordQueueServiceMock.Setup(service => service.GetNextWordAsync(false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(word);
    }

    private void SetupPlayerState(IEnumerable<Guid>? playerIds = null)
    {
        playerIds ??= new Fixture().CreateMany<Guid>();

        playerStateMock.Setup(state => state.PlayerIds).Returns(playerIds);
    }
}
