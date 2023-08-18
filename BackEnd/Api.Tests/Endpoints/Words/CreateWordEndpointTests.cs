using OhMyWord.Api.Endpoints.Words.Create;
using OhMyWord.Api.Models;
using OhMyWord.Api.Tests.Data;
using OhMyWord.Api.Tests.Fixtures;
using OhMyWord.Api.Tests.Priority;
using OhMyWord.Core.Models;
using System.Net;

namespace OhMyWord.Api.Tests.Endpoints.Words;

[Trait("Category", "Integration")]
[Collection("API Integration Tests")]
[TestCaseOrderer("OhMyWord.Api.Tests.Priority.TestPriorityOrderer", "OhMyWord.Api.Tests")]
public class CreateWordEndpointTests
{
    private readonly HttpClient httpClient;

    public CreateWordEndpointTests(ApiWebFactoryFixture fixture)
    {
        httpClient = fixture.CreateClient();
    }

    [Theory]
    [TestPriority(1)]
    [ClassData(typeof(WordsData))]
    public async Task CreateWord_WithValidWord_Should_ReturnCreated(string wordId, PartOfSpeech partOfSpeech,
        string definition, double frequency)
    {
        // arrange
        var request = CreateRequest(wordId, partOfSpeech, definition, frequency);

        // act
        var (message, word) = await httpClient.POSTAsync<CreateWordEndpoint, CreateWordRequest, WordResponse>(request);

        // assert
        message.StatusCode.Should().Be(HttpStatusCode.Created);
        Assert.NotNull(word);
        word.Id.Should().Be(wordId);
        word.Definitions.Should().NotBeEmpty();
        word.Definitions.First().PartOfSpeech.Should().Be(partOfSpeech);
        word.Definitions.First().Value.Should().Be(definition);
        word.Frequency.Should().Be(frequency);
    }

    [Theory]
    [TestPriority(2)]
    [ClassData(typeof(WordsData))]
    public async Task CreateWord_WithExistingWord_Should_Return_Conflict(string wordId, PartOfSpeech partOfSpeech,
        string definition, double frequency)
    {
        // arrange
        var request = CreateRequest(wordId, partOfSpeech, definition, frequency);

        // act
        var message = await httpClient.POSTAsync<CreateWordEndpoint, CreateWordRequest>(request);

        // assert
        message.StatusCode.Should().Be(HttpStatusCode.Conflict);
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

    private static CreateWordRequest CreateRequest(string wordId, PartOfSpeech partOfSpeech, string definition,
        double frequency) =>
        new()
        {
            Id = wordId,
            Definitions = new[] { new Definition { PartOfSpeech = partOfSpeech, Value = definition } },
            Frequency = frequency
        };
}
