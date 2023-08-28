using AutoFixture;
using Microsoft.Extensions.Logging;
using OhMyWord.Core.Models;
using OhMyWord.Core.Services;
using OhMyWord.Data.CosmosDb.Errors;
using OhMyWord.Data.CosmosDb.Services;
using OhMyWord.Data.CosmosDb.Tests.Fixtures;

namespace OhMyWord.Data.CosmosDb.Tests.Services;

[Collection("CosmosDbEmulator")]
[Trait("Category", "Integration")]
public class WordsRepositoryTests : IClassFixture<AutoMapperFixture>
{
    private readonly IWordsRepository wordsRepository;
    private readonly IFixture wordFixture = new Fixture().Customize(new WordCustomization());

    public WordsRepositoryTests(CosmosDbEmulatorFixture fixture, AutoMapperFixture autoMapperFixture)
    {
        wordsRepository = new WordsRepository(fixture.CosmosClient, fixture.Options,
            Mock.Of<ILogger<WordsRepository>>(), autoMapperFixture.Mapper);
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

    [Fact]
    public async Task CreateWordAsync_Should_ReturnExpectedResult()
    {
        // arrange
        var word = wordFixture.Create<Word>();

        // act
        var createResult1 = await wordsRepository.CreateWordAsync(word);
        var readResult = await wordsRepository.GetWordAsync(word.Id);
        var createResult2 = await wordsRepository.CreateWordAsync(word);

        // assert
        createResult1.Should().BeSuccess().Which.Value.Should().BeEquivalentTo(word);
        readResult.Should().BeSuccess().Which.Value.Should().BeEquivalentTo(word);
        createResult2.Should().BeFailure().Which.Should().HaveReason<ItemConflictError>(
            $"Item with ID: {word.Id} already exists on partition: {word.Id}");
    }

    [Fact]
    public async Task DeleteWordAsync_Should_Return_ExpectedResult()
    {
        // arrange
        var wordToCreate = wordFixture.Create<Word>();
        var unknownWordId = wordFixture.Create<string>();

        // act
        var deleteResult1 = await wordsRepository.DeleteWordAsync(unknownWordId);
        var createResult = await wordsRepository.CreateWordAsync(wordToCreate);
        var deleteResult2 = await wordsRepository.DeleteWordAsync(wordToCreate.Id);

        // assert
        deleteResult1.Should().BeFailure().Which.Should().HaveReason<ItemNotFoundError>(
            $"Item with ID: {unknownWordId} was not found on partition: {unknownWordId}");
        createResult.Should().BeSuccess().Which.Value.Should().BeEquivalentTo(wordToCreate);
        deleteResult2.Should().BeSuccess();
    }
}
