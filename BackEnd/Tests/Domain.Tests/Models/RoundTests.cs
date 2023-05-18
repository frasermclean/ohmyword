using AutoFixture.Xunit2;
using OhMyWord.Domain.Models;
using OhMyWord.Infrastructure.Models.Entities;

namespace Domain.Tests.Models;

[Trait("Category", "Unit")]
public class RoundTests
{
    [Theory, AutoData]
    public void NewRound_WithDefaultParameters_Should_HaveExpectedProperties(Word word)
    {
        // arrange
        using var round = new Round(word);

        // assert
        round.Id.Should().BeEmpty();
        round.Number.Should().Be(0);
        round.Word.Should().Be(word);
        round.StartDate.Should().BeBefore(DateTime.UtcNow);
        round.EndDate.Should().BeAfter(DateTime.UtcNow);
        round.EndReason.Should().Be(RoundEndReason.Timeout);
        round.AllPlayersGuessed.Should().BeFalse();
        round.CancellationToken.IsCancellationRequested.Should().BeFalse();
        round.PlayerCount.Should().Be(0);
    }

    [Theory, AutoData]
    public void NewRound_WithSpecifiedParameters_Should_HaveExpectedProperties(Word word, Guid id, int number,
        string[] playerIds)
    {
        // arrange
        using var round = new Round(word, id, number, playerIds);

        // assert
        round.Id.Should().NotBeEmpty();
        round.Number.Should().Be(number);
        round.Word.Should().Be(word);
        round.StartDate.Should().BeBefore(DateTime.UtcNow);
        round.EndDate.Should().BeAfter(DateTime.UtcNow);
        round.EndReason.Should().Be(RoundEndReason.Timeout);
        round.AllPlayersGuessed.Should().BeFalse();
        round.CancellationToken.IsCancellationRequested.Should().BeFalse();
        round.PlayerCount.Should().Be(playerIds.Length);
    }

    [Theory, AutoData]
    public void AddPlayer_Should_IncreasePlayerCount(Word word, int roundNumber, string playerId)
    {
        // arrange
        using var round = new Round(word, number: roundNumber);

        // act
        round.AddPlayer(playerId);

        // assert
        round.PlayerCount.Should().Be(1);
    }

    [Theory, AutoData]
    public void EndRound_Should_SetEndReason(Word word, int roundNumber, RoundEndReason endReason)
    {
        // arrange
        using var round = new Round(word, number: roundNumber);

        // act
        round.EndRound(endReason);

        // assert
        round.EndReason.Should().Be(endReason);
        round.CancellationToken.IsCancellationRequested.Should().BeTrue();
    }

    [Theory, AutoData]
    public void IncrementGuessCount_Should_ReturnCorrectResult(Word word, int roundNumber, string playerId)
    {
        // arrange
        using var round = new Round(word, number: roundNumber, guessLimit: 1);
        round.AddPlayer(playerId);

        // act
        var result1 = round.IncrementGuessCount(playerId);
        var result2 = round.IncrementGuessCount(playerId);
        var result3 = round.IncrementGuessCount("unknownPlayerId");

        // assert
        result1.Should().BeTrue();
        result2.Should().BeFalse();
        result3.Should().BeFalse();
    }

    [Theory, AutoData]
    public void AwardPoints_Should_ReturnExpectedResult(Word word, int roundNumber, string playerId, int points)
    {
        // arrange
        using var round = new Round(word, number: roundNumber);
        round.AddPlayer(playerId);

        // act
        var result1 = round.AwardPoints(playerId, points);
        var result2 = round.AwardPoints("unknownPlayerId", points);

        // assert
        result1.Should().BeTrue();
        result2.Should().BeFalse();
    }

    [Theory, AutoData]
    public void GetPlayerData_Should_ReturnExpectedResult(Word word, int roundNumber, string[] playerIds, int points)
    {
        // arrange
        using var round = new Round(word, number: roundNumber);
        

        // act
        foreach (var playerId in playerIds)
        {
            round.AddPlayer(playerId);
            round.IncrementGuessCount(playerId);
            round.AwardPoints(playerId, points);
        }
        var playerData = round.GetPlayerData().ToArray();

        // assert
        playerData.Length.Should().Be(playerIds.Length);
        playerData.Should().AllSatisfy(data => data.PlayerId.Should().NotBeEmpty());
        playerData.Should().AllSatisfy(data => data.GuessCount.Should().BePositive());
        playerData.Should().AllSatisfy(data => data.PointsAwarded.Should().BePositive());
        playerData.Should().AllSatisfy(data => data.GuessTime.Should().BePositive());
    }

    [Fact]
    public void DefaultRound_Should_HaveExpectedProperties()
    {
        var round = Round.Default;

        // assert
        round.Id.Should().BeEmpty();
        round.Number.Should().Be(0);
        round.Word.Should().Be(Word.Default);
    }
}
