using Microsoft.Extensions.Logging;
using OhMyWord.Core.Models;
using OhMyWord.Domain.Services;

namespace OhMyWord.Domain.Tests.Services;

public class WordQueueServiceTests
{
    private readonly IWordQueueService wordQueueService;
    private readonly IWordsService wordsService = Substitute.For<IWordsService>();

    public WordQueueServiceTests()
    {
        wordQueueService = new WordQueueService(Substitute.For<ILogger<WordQueueService>>(), wordsService);
    }

    [Theory, AutoData]
    public async Task GetNextWordAsync_Should_Return_ExpectedValues(string[] wordIds, Word expectedWord)
    {
        // arrange
        SetupWordService(wordIds, expectedWord);

        // act
        var nextWord = await wordQueueService.GetNextWordAsync();

        // assert
        nextWord.Should().Be(expectedWord);
        wordQueueService.TotalWordCount.Should().Be(wordIds.Length);
        wordQueueService.RemainingWordCount.Should().Be(wordIds.Length - 1);
        await wordsService.Received(1).GetAllWordIds(Arg.Any<CancellationToken>()).ToListAsync();
        await wordsService.Received(1).GetWordAsync(Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
    }


    [Fact]
    public async Task GetNextWordAsync_WithNoWordsInDatabase_Should_Return_DefaultWord()
    {
        // arrange
        SetupWordService();

        // act
        var word = await wordQueueService.GetNextWordAsync();

        // assert
        word.Should().Be(Word.Default);
        await wordsService.Received(1).GetAllWordIds(Arg.Any<CancellationToken>()).ToListAsync();
    }

    private void SetupWordService(IEnumerable<string>? wordIds = default, Word? expectedWord = default)
    {
        wordIds ??= Enumerable.Empty<string>();
        expectedWord ??= Word.Default;

        wordsService.GetAllWordIds(Arg.Any<CancellationToken>())
            .Returns(wordIds.ToAsyncEnumerable());

        wordsService.GetWordAsync(Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(expectedWord);
    }
}
