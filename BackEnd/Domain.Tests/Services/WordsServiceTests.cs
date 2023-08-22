using OhMyWord.Core.Models;
using OhMyWord.Domain.Services;
using OhMyWord.Integrations.Errors;
using OhMyWord.Integrations.Models.Entities;
using OhMyWord.Integrations.Services.Repositories;

namespace OhMyWord.Domain.Tests.Services;

[Trait("Category", "Unit")]
public class WordsServiceTests
{
    private readonly IWordsService wordsService;
    private readonly Mock<IWordsRepository> wordsRepositoryMock = new();
    private readonly Mock<IDefinitionsService> definitionsServiceMock = new();
    private readonly Mock<IWordsApiClient> wordsApiClientMock = new();

    public WordsServiceTests()
    {
        wordsService = new WordsService(wordsRepositoryMock.Object, definitionsServiceMock.Object,
            wordsApiClientMock.Object);
    }

    [Theory, AutoData]
    public async Task CreateWordAsync_WithValidInput_Should_ReturnWord(Word wordToCreate)
    {
        wordsRepositoryMock.Setup(repository =>
                repository.CreateWordAsync(It.IsAny<WordEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((WordEntity entity, CancellationToken _) => entity);

        definitionsServiceMock.Setup(service =>
                service.CreateDefinitionAsync(It.IsAny<string>(), It.IsAny<Definition>(),
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync((string _, Definition definition, CancellationToken _) => definition);

        // act
        var result = await wordsService.CreateWordAsync(wordToCreate);
        var createdWord = result.Value;

        // assert
        result.Should().BeSuccess();
        result.Value.Should().BeOfType<Word>();
        createdWord.Id.Should().Be(wordToCreate.Id);
        createdWord.Definitions.Count().Should().Be(wordToCreate.Definitions.Count());
        createdWord.LastModifiedBy.Should().Be(wordToCreate.LastModifiedBy);
        createdWord.LastModifiedTime.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
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
    public async Task GetWordAsync_WithNoExternalLookup_Should_ReturnExpectedResult(WordEntity wordEntity,
        Definition[] definitions)
    {
        // arrange
        wordsRepositoryMock
            .Setup(repository => repository.GetWordAsync(wordEntity.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(wordEntity);

        definitionsServiceMock
            .Setup(service => service.GetDefinitions(wordEntity.Id, It.IsAny<CancellationToken>()))
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

    [Fact]
    public async Task GetWordAsync_WithExternalLookup_Should_ReturnExpectedResult()
    {
        // arrange
        var fixture = new Fixture().Customize(new DefinitionResultCustomization());
        var wordDetails = fixture.Create<WordDetails>();
        wordsApiClientMock
            .Setup(client => client.GetWordDetailsAsync(wordDetails.Word, It.IsAny<CancellationToken>()))
            .ReturnsAsync(wordDetails);

        // act
        var result = await wordsService.GetWordAsync(wordDetails.Word, true);
        var word = result.Value;

        // assert
        result.Value.Should().BeOfType<Word>();
        result.Should().BeSuccess();
        word.Id.Should().Be(wordDetails.Word);
        word.Definitions.Should().HaveCount(wordDetails.DefinitionResults.Count());
    }

    [Theory, AutoData]
    public async Task GetWordAsync_Should_ReturnNotFound(string wordId)
    {
        // arrange
        wordsRepositoryMock
            .Setup(repository => repository.GetWordAsync(wordId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ItemNotFoundError(wordId, wordId));

        // act
        var result = await wordsService.GetWordAsync(wordId);

        // assert
        result.Should().BeFailure().Which.Should()
            .HaveError($"Item with ID: {wordId} was not found on partition: {wordId}");
    }

    [Theory, AutoData]
    public async Task SearchWords_Should_ReturnExpectedResults(WordEntity[] wordEntities, Definition[] definitions)
    {
        // arrange
        wordsRepositoryMock
            .Setup(x => x.SearchWords(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .Returns(wordEntities.ToAsyncEnumerable());

        definitionsServiceMock
            .Setup(service => service.GetDefinitions(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(definitions.ToAsyncEnumerable());

        // act
        var words = await wordsService.SearchWords().ToListAsync();

        // assert
        words.Should().HaveCount(wordEntities.Length);
        words.Should().AllSatisfy(word => word.Definitions.Should().HaveCount(definitions.Length));
    }

    [Theory, AutoData]
    public async Task UpdateWord_Should_Return_ExpectedResult(Word word)
    {
        // arrange
        wordsRepositoryMock
            .Setup(repository => repository.UpdateWordAsync(It.IsAny<WordEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((WordEntity entity, CancellationToken _) => entity);

        definitionsServiceMock
            .Setup(service => service.UpdateDefinitionAsync(It.IsAny<string>(), It.IsAny<Definition>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((string _, Definition definition, CancellationToken _) => definition);

        // act
        var result = await wordsService.UpdateWordAsync(word);

        // assert
        result.Should().BeSuccess();
    }

    private class DefinitionResultCustomization : ICustomization
    {
        public void Customize(IFixture fixture)
        {
            fixture.Customize<DefinitionResult>(composer => composer.With(result => result.PartOfSpeech, "noun"));
        }
    }
}
