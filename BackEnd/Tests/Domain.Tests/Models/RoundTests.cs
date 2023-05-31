using OhMyWord.Domain.Models;
using OhMyWord.Domain.Options;
using OhMyWord.Infrastructure.Models.Entities;

namespace Domain.Tests.Models;

[Trait("Category", "Unit")]
public class RoundTests
{
    [Theory, AutoData]
    public void NewRound_WithDefaultParameters_Should_HaveExpectedProperties(Word word)
    {
        // arrange
        using var round = CreateRound(word);

        // assert
        round.Id.Should().NotBeEmpty();
        round.Number.Should().Be(0);
        round.Word.Should().Be(word);
        round.StartDate.Should().BeBefore(DateTime.UtcNow);
        round.EndDate.Should().BeAfter(DateTime.UtcNow);
        round.EndReason.Should().BeNull();
        round.AllPlayersGuessed.Should().BeFalse();
        round.CancellationToken.IsCancellationRequested.Should().BeFalse();
        round.PlayerCount.Should().Be(0);
        round.SessionId.Should().BeEmpty();
        round.IsActive.Should().BeTrue();
    }

    [Theory, AutoData]
    public void NewRound_WithSpecifiedParameters_Should_HaveExpectedProperties(Word word, int number,
        Guid[] playerIds, Guid sessionId)
    {
        // arrange
        using var round = CreateRound(word, number, playerIds, sessionId);

        // assert
        round.Id.Should().NotBeEmpty();
        round.Number.Should().Be(number);
        round.Word.Should().Be(word);
        round.StartDate.Should().BeBefore(DateTime.UtcNow);
        round.EndDate.Should().BeAfter(DateTime.UtcNow);
        round.EndReason.Should().BeNull();
        round.AllPlayersGuessed.Should().BeFalse();
        round.CancellationToken.IsCancellationRequested.Should().BeFalse();
        round.PlayerCount.Should().Be(playerIds.Length);
        round.SessionId.Should().Be(sessionId);
        round.IsActive.Should().BeTrue();
    }

    [Theory, AutoData]
    public void AddPlayer_Should_IncreasePlayerCount(Word word, int roundNumber, Guid playerId)
    {
        // arrange
        using var round = CreateRound(word, roundNumber);

        // act
        var result = round.AddPlayer(playerId);

        // assert
        round.PlayerCount.Should().Be(1);
        result.Should().BeTrue();
    }

    [Theory, AutoData]
    public void EndRound_Should_SetEndReason(Word word, int roundNumber, RoundEndReason endReason)
    {
        // arrange
        using var round = CreateRound(word, roundNumber);

        // act
        round.EndRound(endReason);

        // assert
        round.EndReason.Should().Be(endReason);
        round.CancellationToken.IsCancellationRequested.Should().BeTrue();
        round.IsActive.Should().BeFalse();
    }

    [Theory, AutoData]
    public void ProcessGuess_Should_ReturnCorrectResult(Word word, int roundNumber, Guid playerId, string incorrectGuess,
        Guid invalidPlayerId, Guid invalidRoundId)
    {
        // arrange
        using var round = CreateRound(word, roundNumber, guessLimit: 2);
        round.AddPlayer(playerId);        

        // act
        var result1 = round.ProcessGuess(playerId, round.Id, word.Id);
        var result2 = round.ProcessGuess(invalidPlayerId, round.Id, word.Id);
        var result3 = round.ProcessGuess(playerId, invalidRoundId, word.Id);
        var result4 = round.ProcessGuess(playerId, round.Id, incorrectGuess);
        var result5 = round.ProcessGuess(playerId, round.Id, word.Id);

        // assert
        result1.Should().BeSuccess();
        result2.Should().BeFailure()
            .Which.Should().HaveError($"Player with ID: {invalidPlayerId} is not in round");
        result3.Should().BeFailure()
            .Which.Should().HaveError("Round is inactive or ID does not match");
        result4.Should().BeFailure()
            .Which.Should().HaveError($"Guess value of '{incorrectGuess}' is incorrect");
        result5.Should().BeFailure()
            .Which.Should().HaveError($"Guess limit: 2 exceeded for player with ID: {playerId}");
    }


    [Theory, AutoData]
    public void GetPlayerData_Should_ReturnExpectedResult(Word word, int roundNumber, Guid[] playerIds)
    {
        // arrange
        using var round = CreateRound(word, roundNumber);

        // act
        foreach (var playerId in playerIds)
        {
            round.AddPlayer(playerId);
            round.ProcessGuess(playerId, round.Id, word.Id);
        }

        var playerData = round.GetPlayerData().ToArray();

        // assert
        playerData.Length.Should().Be(playerIds.Length);
        playerData.Should().AllSatisfy(data => data.PlayerId.Should().NotBeEmpty());
        playerData.Should().AllSatisfy(data => data.GuessCount.Should().BePositive());
        playerData.Should().AllSatisfy(data => data.PointsAwarded.Should().BePositive());
        playerData.Should().AllSatisfy(data => data.GuessTime.Should().BePositive());
    }

    private static Round CreateRound(Word word, int number = default, IEnumerable<Guid>? playerIds = default,
        Guid sessionId = default, double letterHintDelay = RoundOptions.LetterHintDelayDefault,
        int guessLimit = RoundOptions.GuessLimitDefault) =>
        new(word, TimeSpan.FromSeconds(letterHintDelay), playerIds)
        {
            Number = number, GuessLimit = guessLimit, SessionId = sessionId
        };
}
