using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using OhMyWord.Core.Game;
using OhMyWord.Data.Models;
using OhMyWord.Data.Services;
using Xunit;

namespace Core.Tests;

public class WordsServiceTests
{
    private readonly Mock<IWordsRepository> wordsRepositoryMock;
    private readonly IWordsService wordsService;

    public WordsServiceTests()
    {
        var loggerMock = new Mock<ILogger<WordsService>>();
        wordsRepositoryMock = new Mock<IWordsRepository>();
        wordsService = new WordsService(loggerMock.Object, wordsRepositoryMock.Object);
    }

    [Fact]
    public async Task GetNextWord_WithTestWords_Should_ReturnValidWord()
    {
        // arrange
        wordsRepositoryMock.Setup(repository => repository.GetAllWordsAsync(default))
            .ReturnsAsync(TestWords);

        // act
        var word = await wordsService.GetNextWordAsync();

        // assert
        word.Should().NotBe(Word.Default);
        word.Should().BeOfType<Word>();
        wordsService.RemainingWordCount.Should().Be(TestWords.Length - 1);
        TestWords.Should().Contain(word);
        wordsRepositoryMock.Verify(repository => repository.GetAllWordsAsync(default), Times.Once);
    }

    [Fact]
    public async Task GetNextWord_WithNoWords_Should_ReturnDefaultWord()
    {
        // arrange
        wordsRepositoryMock.Setup(repository => repository.GetAllWordsAsync(default))
            .ReturnsAsync(Enumerable.Empty<Word>());

        // act
        var word = await wordsService.GetNextWordAsync();

        // assert
        word.Should().Be(Word.Default);
        TestWords.Should().NotContain(word);
        wordsRepositoryMock.Verify(repository => repository.GetAllWordsAsync(default), Times.Once);
    }

    [Fact]
    public async Task Setting_ShouldReloadWords_Should_ReloadWords()
    {
        // arrange
        wordsRepositoryMock.Setup(repository => repository.GetAllWordsAsync(default))
            .ReturnsAsync(TestWords);
        
        // act
        wordsService.ShouldReloadWords = true;
        var word = await wordsService.GetNextWordAsync();
        
        // assert
        word.Should().NotBe(Word.Default);
        word.Should().BeOfType<Word>();
        TestWords.Should().Contain(word);
        wordsRepositoryMock.Verify(repository => repository.GetAllWordsAsync(default), Times.Once);
    }

    private static readonly Word[] TestWords =
    {
        new()
        {
            Value = "dog",
            Definition = "Man's best friend",
        },
        new()
        {
            Value = "tree",
            Definition = "Green and leafy"
        },
        new()
        {
            Value = "house",
            Definition = "Building where you live"
        }
    };
}