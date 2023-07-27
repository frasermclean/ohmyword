using OhMyWord.Api.Endpoints.Words.Create;
using OhMyWord.Api.Tests.Fixtures;
using OhMyWord.Domain.Models;
using OhMyWord.Infrastructure.Models.Entities;
using System.Net;

namespace OhMyWord.Api.Tests.Endpoints.Words;

[Trait("Category", "Integration")]
[Collection("API Integration Tests")]
public class CreateWordEndpointTests
{
    private readonly HttpClient httpClient;

    public CreateWordEndpointTests(ApiWebFactoryFixture fixture)
    {
        httpClient = fixture.CreateClient();
    }

    [Theory]
    [InlineData("house", PartOfSpeech.Noun, "Building for human habitation")]
    public async Task CreateWord_WithValidWord_Should_ReturnCreated(string wordId, PartOfSpeech partOfSpeech,
        string definition)
    {
        // arrange
        var request = new CreateWordRequest
        {
            Id = wordId, Definitions = new[] { new Definition { PartOfSpeech = partOfSpeech, Value = definition } }
        };

        // act
        var (message, word) = await httpClient.POSTAsync<CreateWordEndpoint, CreateWordRequest, Word>(request);

        // assert
        message.StatusCode.Should().Be(HttpStatusCode.Created);
        Assert.NotNull(word);
        word.Id.Should().Be(wordId);
        word.Definitions.Should().NotBeEmpty();
        word.Definitions.First().PartOfSpeech.Should().Be(partOfSpeech);
        word.Definitions.First().Value.Should().Be(definition);
    }

    [Fact]
    public async Task CreateWord_WithInvalidRequest_Should_Return_BadRequest()
    {
        // arrange
        var request = new CreateWordRequest();

        // act
        var (message, errorResponse) =
            await httpClient.POSTAsync<CreateWordEndpoint, CreateWordRequest, ErrorResponse>(request);

        // assert
        message.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        Assert.NotNull(errorResponse);
        errorResponse.Errors.Should().NotBeEmpty();
    }
}
