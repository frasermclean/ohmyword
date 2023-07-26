using OhMyWord.Api.Endpoints.Words.Create;
using OhMyWord.Api.Tests.Fixtures;
using OhMyWord.Domain.Models;
using OhMyWord.Infrastructure.Models.Entities;
using System.Net;

namespace OhMyWord.Api.Tests.Endpoints.Words;

[Trait("Category", "Integration")]
public class CreateWordEndpointTests : IClassFixture<ApiWebFactory>
{
    private readonly HttpClient httpClient;

    public CreateWordEndpointTests(ApiWebFactory factory)
    {
        httpClient = factory.CreateClient();
    }

    [Theory]
    [InlineData("test", PartOfSpeech.Noun)]
    public async Task CreateWord_WithValidWord_Should_ReturnCreated(string wordId, PartOfSpeech partOfSpeech)
    {
        // arrange
        var request = new CreateWordRequest
        {
            Id = wordId, Definitions = new[] { new Definition { PartOfSpeech = partOfSpeech, Value = "test" } }
        };

        // act
        var (message, word) = await httpClient.POSTAsync<CreateWordEndpoint, CreateWordRequest, Word>(request);

        // assert
        message.StatusCode.Should().Be(HttpStatusCode.Created);
        Assert.NotNull(word);
        word.Id.Should().Be(wordId);
    }
}
