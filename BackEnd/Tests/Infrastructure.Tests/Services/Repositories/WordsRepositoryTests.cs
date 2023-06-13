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
    public async Task CreateWordAsync_Should_ReturnExpectedResult(WordEntity entity)
    {
        // act
        var result1 = await wordsRepository.CreateWordAsync(entity);
        var result2 = await wordsRepository.CreateWordAsync(entity);

        // assert
        result1.Should().BeSuccess();
        result1.Value.Id.Should().Be(entity.Id);
        result1.Value.DefinitionCount.Should().Be(entity.DefinitionCount);
        result2.Should().BeFailure().Which.Should().HaveReason<ItemConflictError>(
            $"Item with ID: {entity.Id} already exists on partition: {entity.Id}");
    }
}
