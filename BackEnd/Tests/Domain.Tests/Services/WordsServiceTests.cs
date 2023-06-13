using Microsoft.Extensions.Logging;
using OhMyWord.Domain.Models;
using OhMyWord.Domain.Services;
using OhMyWord.Infrastructure.Errors;
using OhMyWord.Infrastructure.Models.Entities;
using OhMyWord.Infrastructure.Services.Repositories;

namespace Domain.Tests.Services;

[Trait("Category", "Unit")]
public class WordsServiceTests
{
    private readonly IWordsService wordsService;
    private readonly Mock<ILogger<WordsService>> loggerMock = new();
    private readonly Mock<IWordsRepository> wordsRepositoryMock = new();
    private readonly Mock<IDefinitionsService> definitionsServiceMock = new();

    public WordsServiceTests()
    {
        wordsService =
            new WordsService(loggerMock.Object, wordsRepositoryMock.Object, definitionsServiceMock.Object);
    }

    [Theory, AutoData]
    public async Task CreateWordAsync_WithValidInput_Should_ReturnWord(Word word)
    {
        wordsRepositoryMock.Setup(repository =>
                repository.CreateWordAsync(It.IsAny<WordEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((WordEntity entity, CancellationToken _) => entity);

        definitionsServiceMock.Setup(service =>
                service.CreateDefinitionAsync(It.IsAny<string>(), It.IsAny<Definition>(),
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync((string _, Definition definition, CancellationToken _) => definition);

        // act
        var result = await wordsService.CreateWordAsync(word);
        var createdWord = result.Value;

        // assert
        result.Should().BeSuccess();
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
            .ReturnsAsync((WordEntity entity, CancellationToken _) => new ItemConflictError(entity.Id, entity.Id));

        // act
        var result = await wordsService.CreateWordAsync(word);

        // assert
        result.Should().BeFailure().Which.Should()
            .HaveReason<ItemConflictError>($"Item with ID: {word.Id} already exists on partition: {word.Id}");
    }

    [Theory, AutoData]
    public async Task GetWordAsync_Should_ReturnWord(WordEntity wordEntity, Definition[] definitions)
    {
        wordsRepositoryMock
            .Setup(repository => repository.GetWordAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(wordEntity);

        definitionsServiceMock
            .Setup(service => service.GetDefinitions(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(definitions.ToAsyncEnumerable());

        // act
        var result = await wordsService.GetWordAsync(wordEntity.Id);
        var word = result.Value;

        // assert
        result.Value.Should().BeOfType<Word>();
        result.Should().BeSuccess();
        word.Id.Should().Be(wordEntity.Id);
        word.Definitions.Should().HaveCount(definitions.Length);
    }

    [Theory, AutoData]
    public async Task GetWordAsync_Should_ReturnNotFound(string wordId)
    {
        // arrange
        wordsRepositoryMock
            .Setup(repository => repository.GetWordAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ItemNotFoundError(wordId, wordId));

        // act
        var result = await wordsService.GetWordAsync(wordId);

        // assert
        result.Should().BeFailure().Which.Should()
            .HaveError($"Item with ID: {wordId} was not found on partition: {wordId}");
    }
}
