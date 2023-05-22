using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using OhMyWord.Domain.Models;
using OhMyWord.Domain.Results;
using OhMyWord.Domain.Services;
using OhMyWord.Infrastructure.Models.Entities;
using OhMyWord.Infrastructure.Services;
using OneOf.Types;
using System.Net;

namespace Domain.Tests.Services;

[Trait("Category", "Unit")]
public class WordsServiceTests
{
    private readonly IWordsService wordsService;
    private readonly Mock<ILogger<WordsService>> loggerMock = new();
    private readonly Mock<IWordsRepository> wordsRepositoryMock = new();
    private readonly Mock<IDefinitionsRepository> definitionsRepositoryMock = new();

    public WordsServiceTests()
    {
        wordsService =
            new WordsService(loggerMock.Object, wordsRepositoryMock.Object, definitionsRepositoryMock.Object);
    }

    [Theory, AutoData]
    public async Task CreateWordAsync_WithValidInput_Should_ReturnWord(Word word)
    {
        // arrange
        wordsRepositoryMock.Setup(repository =>
                repository.CreateWordAsync(It.IsAny<WordEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new WordEntity { Id = word.Id, DefinitionCount = word.Definitions.Count() });

        // act
        var result = await wordsService.CreateWordAsync(word);
        var createdWord = result.AsT0;

        // assert
        result.Value.Should().BeOfType<Word>();
        createdWord.Id.Should().Be(word.Id);
        createdWord.Definitions.Count().Should().Be(word.Definitions.Count());
    }

    [Theory, AutoData]
    public async Task CreateWordAsync_WithConflictingInput_Should_ReturnConflict(Word word)
    {
        // arrange
        wordsRepositoryMock.Setup(repository =>
                repository.CreateWordAsync(It.IsAny<WordEntity>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new CosmosException("Conflict", HttpStatusCode.Conflict, 0, "", 0));

        // act
        var result = await wordsService.CreateWordAsync(word);

        // assert
        result.Value.Should().BeOfType<Conflict>();
    }

    [Theory, AutoData]
    public async Task GetWordAsync_Should_ReturnWord(WordEntity wordEntity, DefinitionEntity[] definitionEntities)
    {
        wordsRepositoryMock
            .Setup(repository => repository.GetWordAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(wordEntity);

        definitionsRepositoryMock
            .Setup(repository => repository.GetDefinitionsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(definitionEntities.ToAsyncEnumerable());

        // act
        var result = await wordsService.GetWordAsync(wordEntity.Id);
        var word = result.AsT0;

        // assert
        result.Value.Should().BeOfType<Word>();
        word.Id.Should().Be(wordEntity.Id);
        word.Definitions.Should().HaveCount(definitionEntities.Length);
    }

    [Theory, AutoData]
    public async Task GetWordAsync_Should_ReturnNotFound(string wordId)
    {
        // arrange
        wordsRepositoryMock
            .Setup(repository => repository.GetWordAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(null as WordEntity);

        // act
        var result = await wordsService.GetWordAsync(wordId);

        // assert
        result.Value.Should().BeOfType<NotFound>();
    }
}
