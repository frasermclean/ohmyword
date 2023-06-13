using Infrastructure.Tests.Fixtures;
using Microsoft.Extensions.Logging;
using OhMyWord.Infrastructure.Errors;
using OhMyWord.Infrastructure.Models.Entities;
using OhMyWord.Infrastructure.Options;
using OhMyWord.Infrastructure.Services;

namespace Infrastructure.Tests.Services.Repositories;

[Collection("CosmosDbEmulator")]
[Trait("Category", "Integration")]
public class WordsRepositoryTests
{
    private readonly IWordsRepository wordsRepository;

    public WordsRepositoryTests(CosmosDbEmulatorFixture fixture)
    {
        var client = fixture.CosmosClient;
        var options = Microsoft.Extensions.Options.Options.Create(new CosmosDbOptions { DatabaseId = "test-db" });

        wordsRepository = new WordsRepository(client, options, Mock.Of<ILogger<WordsRepository>>());
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
}
