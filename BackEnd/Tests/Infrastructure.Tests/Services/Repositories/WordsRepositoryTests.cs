using Infrastructure.Tests.Fixtures;
using Microsoft.Extensions.Logging;
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

    [Fact]
    public async Task Test()
    {
        // arrange
        var entity = new WordEntity { Id = "test" };
        
        // act
        var result = await wordsRepository.CreateWordAsync(entity);
        
        // assert
    }
}
