using Microsoft.Extensions.Logging;
using OhMyWord.Integrations.Services.Repositories;
using OhMyWord.Integrations.Tests.Fixtures;

namespace OhMyWord.Integrations.Tests.Services.Repositories;

[Collection("CosmosDbEmulator")]
[Trait("Category", "Integration")]
public class DefinitionsRepositoryTests
{
    private readonly IDefinitionsRepository definitionsRepository;

    public DefinitionsRepositoryTests(CosmosDbEmulatorFixture fixture)
    {
        var logger = Substitute.For<ILogger<DefinitionsRepository>>();
        definitionsRepository = new DefinitionsRepository(fixture.CosmosClient, fixture.Options, logger);
    }

    [Fact]
    public async Task GetDefinitions_WithRandomWordId_Should_Return_Empty()
    {
        // arrange
        var wordId = Guid.NewGuid().ToString();

        // act
        var definitions = await definitionsRepository.GetDefinitions(wordId).ToListAsync();

        // assert
        definitions.Should().BeEmpty();
    }
}
