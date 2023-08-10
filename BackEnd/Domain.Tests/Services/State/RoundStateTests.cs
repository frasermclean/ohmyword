// using Microsoft.Extensions.Logging;
// using OhMyWord.Core.Models;
// using OhMyWord.Domain.Models;
// using OhMyWord.Domain.Services;
// using OhMyWord.Domain.Services.State;
//
// namespace OhMyWord.Domain.Tests.Services.State;
//
// public class RoundStateTests
// {
//     private readonly IRoundState roundState;
//     private readonly Mock<IRoundService> roundServiceMock = new();
//
//     public RoundStateTests()
//     {
//         var sessionState = new SessionState(Mock.Of<ILogger<SessionState>>());
//         roundState = new RoundState(Mock.Of<ILogger<RoundState>>(), sessionState, roundServiceMock.Object);
//     }
//
//     [Fact]
//     public async Task CreateRoundAsync_Should_Return_ExpectedResult()
//     {
//         // arrange
//         var round = SetupRoundServiceMock();
//
//         // act
//         var startData = await roundState.CreateRoundAsync();
//
//         // assert
//         startData.RoundNumber.Should().Be(round.Number);
//         startData.RoundId.Should().Be(round.Id);
//         startData.WordHint.Length.Should().Be(round.WordHint.Length);
//         startData.StartDate.Should().Be(round.StartDate);
//         startData.EndDate.Should().Be(round.EndDate);
//     }
//
//     [Fact]
//     public async Task ExecuteRoundAsync_Should_Set_ExpectedValues()
//     {
//         // arrange
//         var round = SetupRoundServiceMock();
//
//         // act
//         await roundState.CreateRoundAsync();
//         var summary = await roundState.ExecuteRoundAsync();
//
//         // assert
//         summary.Word.Should().Be(round.Word.Id);
//         summary.DefinitionId.Should().NotBeEmpty();
//         summary.RoundId.Should().Be(round.Id);
//     }
//
//     [Fact]
//     public async Task GetSnapshot_Should_ReturnExpectedValues()
//     {
//         // arrange
//         var round = SetupRoundServiceMock();
//
//         // act
//         await roundState.CreateRoundAsync();
//         var snapshot = roundState.GetSnapshot();
//
//         // assert
//         snapshot.RoundActive.Should().BeTrue();
//         snapshot.RoundNumber.Should().Be(round.Number);
//         snapshot.RoundId.Should().Be(round.Id);
//         snapshot.Interval.StartDate.Should().Be(round.StartDate);
//         snapshot.Interval.EndDate.Should().Be(round.EndDate);
//         snapshot.WordHint.Should().Be(round.WordHint);
//     }
//
//     [Theory, AutoData]
//     public async Task ProcessGuess_Should_ReturnCorrectResult(Guid playerId1, Guid playerId2, Guid invalidPlayerId,
//         Guid invalidRoundId, string incorrectGuess)
//     {
//         // arrange
//         var round = SetupRoundServiceMock();
//         round.EndDate = DateTime.MaxValue;
//         round.PlayerData[playerId1] = new RoundPlayerData();
//         round.PlayerData[playerId2] = new RoundPlayerData { GuessCount = 3 };
//         var correctGuess = round.Word.Id;
//
//         // act
//         var startData = await roundState.CreateRoundAsync();
//         var result1 = roundState.ProcessGuess(invalidPlayerId, startData.RoundId, correctGuess);
//         var result2 = roundState.ProcessGuess(playerId1, invalidRoundId, correctGuess);
//         var result3 = roundState.ProcessGuess(playerId1, startData.RoundId, incorrectGuess);
//         var result4 = roundState.ProcessGuess(playerId2, startData.RoundId, correctGuess);
//         var result5 = roundState.ProcessGuess(playerId1, startData.RoundId, correctGuess);
//
//         // assert
//         result1.Should().BeFailure()
//             .Which.Should().HaveError($"Player with ID: {invalidPlayerId} is not in round");
//         result2.Should().BeFailure()
//             .Which.Should().HaveError("Round is inactive or ID does not match");
//         result3.Should().BeFailure()
//             .Which.Should().HaveError($"Guess value of '{incorrectGuess}' is incorrect");
//         result4.Should().BeFailure()
//             .Which.Should().HaveError($"Guess limit: {round.GuessLimit} exceeded for player with ID: {playerId2}");
//         result5.Should().BeSuccess();
//     }
//
//     private Round SetupRoundServiceMock()
//     {
//         var fixture = new Fixture();
//         var round = fixture.Build<Round>()
//             .With(r => r.GuessLimit, 3)
//             .With(r => r.EndDate, DateTime.UtcNow + TimeSpan.FromSeconds(5))
//             .Create();
//
//         roundServiceMock.Setup(service => service.CreateRoundAsync(It.IsAny<int>(), It.IsAny<Guid>(),
//                 It.IsAny<bool>(), It.IsAny<CancellationToken>()))
//             .ReturnsAsync(round);
//
//         roundServiceMock.Setup(service => service.ExecuteRoundAsync(round, It.IsAny<CancellationToken>()))
//             .ReturnsAsync(new RoundSummary
//             {
//                 Word = round.Word.Id,
//                 PartOfSpeech = PartOfSpeech.Noun,
//                 EndReason = RoundEndReason.Timeout,
//                 RoundId = round.Id,
//                 DefinitionId = Guid.NewGuid(),
//                 NextRoundStart = DateTime.UtcNow + TimeSpan.FromSeconds(5),
//                 Scores = Enumerable.Empty<ScoreLine>()
//             });
//
//         return round;
//     }
// }


