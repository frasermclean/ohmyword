// using FluentResults;
// using Microsoft.Extensions.Logging;
// using OhMyWord.Core.Models;
// using OhMyWord.Domain.Services;
//
// namespace OhMyWord.Domain.Tests.Services;
//
// public class WordQueueServiceTests
// {
//     private readonly IWordQueueService wordQueueService;
//     private readonly Mock<IWordsService> wordsServiceMock = new();
//
//     public WordQueueServiceTests()
//     {
//         wordQueueService = new WordQueueService(Mock.Of<ILogger<WordQueueService>>(), wordsServiceMock.Object);
//     }
//
//     [Theory, AutoData]
//     public async Task GetNextWordAsync_Should_Return_ExpectedValues(string[] wordIds, Word expectedWord)
//     {
//         // arrange
//         wordsServiceMock.Setup(service => service.GetAllWordIds(It.IsAny<CancellationToken>()))
//             .Returns(wordIds.ToAsyncEnumerable());
//         wordsServiceMock.Setup(service =>
//                 service.GetWordAsync(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
//             .ReturnsAsync(Result.Ok(expectedWord));
//
//         // act
//         var nextWord = await wordQueueService.GetNextWordAsync();
//
//         // assert
//         nextWord.Should().Be(expectedWord);
//         wordQueueService.TotalWordCount.Should().Be(wordIds.Length);
//         wordQueueService.RemainingWordCount.Should().Be(wordIds.Length - 1);
//         wordsServiceMock.Verify(service => service.GetAllWordIds(It.IsAny<CancellationToken>()), Times.Once);
//         wordsServiceMock.Verify(
//             service => service.GetWordAsync(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()),
//             Times.Once);
//     }
//
//     [Fact]
//     public async Task GetNextWordAsync_WithNoWordsInDatabase_Should_Return_DefaultWord()
//     {
//         // arrange
//         wordsServiceMock.Setup(service => service.GetAllWordIds(It.IsAny<CancellationToken>()))
//             .Returns(AsyncEnumerable.Empty<string>());
//
//         // act
//         var word = await wordQueueService.GetNextWordAsync();
//
//         // assert
//         word.Should().Be(Word.Default);
//         wordsServiceMock.Verify(service => service.GetAllWordIds(It.IsAny<CancellationToken>()), Times.Once);
//     }
// }


