using Infrastructure.Tests.Fixtures;
using Microsoft.Extensions.Logging;
using OhMyWord.Infrastructure.Services.Repositories;

namespace Infrastructure.Tests.Services.Repositories;

[Collection("CosmosDbEmulator")]
[Trait("Category", "Integration")]
public class DefinitionsRepositoryTests
{
    private readonly IDefinitionsRepository definitionsRepository;

    public DefinitionsRepositoryTests(CosmosDbEmulatorFixture fixture)
    {
        definitionsRepository =
            new DefinitionsRepository(fixture.CosmosClient, fixture.Options, Mock.Of<ILogger<DefinitionsRepository>>());
    }

    [Fact]
    public async Task GetDefinitions_WithRandomWordId_Should_Return_Empty()
    {
        // arrange
        var wordId = Guid.NewGuid().ToString();
        
        // act
        var definitions = await definitionsRepository.GetDefinitions(wordId).ToListAsync();
        
        // assert
        definitions.Count.Should().Be(0);
    }
}
