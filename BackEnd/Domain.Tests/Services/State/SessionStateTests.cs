// using Microsoft.Extensions.Logging;
// using OhMyWord.Domain.Services.State;
//
// namespace OhMyWord.Domain.Tests.Services.State;
//
// public class SessionStateTests
// {
//     private readonly ISessionState sessionState;
//
//     public SessionStateTests()
//     {
//         sessionState = new SessionState(Mock.Of<ILogger<SessionState>>());
//     }
//
//     [Fact]
//     public void DefaultState_Should_Have_ExpectedValues()
//     {
//         // assert
//         sessionState.SessionId.Should().Be(Guid.Empty);
//         sessionState.RoundCount.Should().Be(0);
//         sessionState.IsDefault.Should().BeTrue();
//     }
//
//     [Fact]
//     public void NextSession_Should_Set_ExpectedValues()
//     {
//         // act
//         var session = sessionState.NextSession();
//
//         // assert
//         session.Should().NotBeNull();
//         sessionState.SessionId.Should().Be(session.Id);
//         sessionState.RoundCount.Should().Be(0);
//         sessionState.IsDefault.Should().BeFalse();
//     }
//
//     [Fact]
//     public void IncrementRoundCount_Should_Return_ExpectedResults()
//     {
//         // act
//         sessionState.NextSession();
//         var roundCount1 = sessionState.IncrementRoundCount();
//         var roundCount2 = sessionState.IncrementRoundCount();
//
//         // assert
//         roundCount1.Should().Be(1);
//         roundCount2.Should().Be(2);
//         sessionState.RoundCount.Should().Be(2);
//     }
//
//     [Fact]
//     public void Reset_Should_Set_ExpectedValues()
//     {
//         // act
//         sessionState.NextSession();
//         sessionState.IncrementRoundCount();
//         sessionState.Reset();
//
//         // assert
//         sessionState.SessionId.Should().Be(Guid.Empty);
//         sessionState.RoundCount.Should().Be(0);
//         sessionState.IsDefault.Should().BeTrue();
//     }
// }
