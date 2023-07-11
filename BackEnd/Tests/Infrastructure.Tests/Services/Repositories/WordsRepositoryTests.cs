using Infrastructure.Tests.Fixtures;
using Microsoft.Extensions.Logging;
using OhMyWord.Infrastructure.Errors;
using OhMyWord.Infrastructure.Models.Entities;
using OhMyWord.Infrastructure.Services.Repositories;

namespace Infrastructure.Tests.Services.Repositories;

[Collection("CosmosDbEmulator")]
[Trait("Category", "Integration")]
public class WordsRepositoryTests
{
    private readonly IWordsRepository wordsRepository;

    public WordsRepositoryTests(CosmosDbEmulatorFixture fixture)
    {
        wordsRepository =
            new WordsRepository(fixture.CosmosClient, fixture.Options, Mock.Of<ILogger<WordsRepository>>());
    }

    [Theory, AutoData]
    public async Task GetWordAsync_WithUnknownWordId_Should_Return_NotFound(string wordId)
    {
        // act
        var result = await wordsRepository.GetWordAsync(wordId);

        // assert
        result.Should().BeFailure().Which.Should().HaveReason<ItemNotFoundError>(
            $"Item with ID: {wordId} was not found on partition: {wordId}");
    }

    [Theory, AutoData]
    public async Task CreateWordAsync_Should_ReturnExpectedResult(WordEntity entity)
    {
        // act
        var createResult1 = await wordsRepository.CreateWordAsync(entity);
        var readResult = await wordsRepository.GetWordAsync(entity.Id);
        var createResult2 = await wordsRepository.CreateWordAsync(entity);

        // assert
        createResult1.Should().BeSuccess();
        createResult1.Value.Id.Should().Be(entity.Id);
        createResult1.Value.DefinitionCount.Should().Be(entity.DefinitionCount);
        readResult.Should().BeSuccess();
        readResult.Value.Id.Should().Be(entity.Id);
        readResult.Value.DefinitionCount.Should().Be(entity.DefinitionCount);
        createResult2.Should().BeFailure().Which.Should().HaveReason<ItemConflictError>(
            $"Item with ID: {entity.Id} already exists on partition: {entity.Id}");
    }

    [Theory, AutoData]
    public async Task DeleteWordAsync_Should_Return_ExpectedResult(string unknownWordId, WordEntity entityToCreate)
    {
        // act
        var deleteResult1 = await wordsRepository.DeleteWordAsync(unknownWordId);
        var createResult = await wordsRepository.CreateWordAsync(entityToCreate);
        var deleteResult2 = await wordsRepository.DeleteWordAsync(entityToCreate.Id);

        // assert
        deleteResult1.Should().BeFailure().Which.Should().HaveReason<ItemNotFoundError>(
            $"Item with ID: {unknownWordId} was not found on partition: {unknownWordId}");
        createResult.Should().BeSuccess();
        deleteResult2.Should().BeSuccess();
    }
}
