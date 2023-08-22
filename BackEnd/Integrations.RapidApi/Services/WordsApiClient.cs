using Microsoft.Extensions.Logging;
using OhMyWord.Core.Models;
using OhMyWord.Core.Services;
using OhMyWord.Integrations.RapidApi.Models.WordsApi;
using System.Net;
using System.Net.Http.Json;

namespace OhMyWord.Integrations.RapidApi.Services;

/// <summary>
/// WordsAPI client implementation.
/// https://rapidapi.com/dpventures/api/wordsapi/
/// </summary>
public class WordsApiClient : IDictionaryClient
{
    private readonly ILogger<WordsApiClient> logger;
    private readonly HttpClient httpClient;

    public WordsApiClient(ILogger<WordsApiClient> logger, HttpClient httpClient)
    {
        this.logger = logger;
        this.httpClient = httpClient;
    }

    public Task<Word?> GetWordAsync(string word, CancellationToken cancellationToken)
        => SendRequestAsync(new Uri(word, UriKind.Relative), cancellationToken);

    public async Task<Word> GetRandomWordAsync(CancellationToken cancellationToken = default)
        => await SendRequestAsync(new Uri("?random=true", UriKind.Relative), cancellationToken) ??
           throw new InvalidOperationException("Random word return null reference");

    private async Task<Word?> SendRequestAsync(Uri uri, CancellationToken cancellationToken)
    {
        var message = await httpClient.GetAsync(uri, cancellationToken);

        if (message.StatusCode == HttpStatusCode.NotFound)
        {
            logger.LogWarning("Found no details for word: {Uri}", uri);
            return default;
        }

        message.EnsureSuccessStatusCode();

        var wordDetails = await message.Content.ReadFromJsonAsync<WordDetails>(cancellationToken: cancellationToken);

        return wordDetails is null ? default : MapToWord(wordDetails);
    }

    private static Word MapToWord(WordDetails details) => new()
    {
        Id = details.Word,
        Definitions = details.DefinitionResults.Select(result => new Definition
        {
            Id = Guid.NewGuid(),
            PartOfSpeech = Enum.Parse<PartOfSpeech>(result.PartOfSpeech, true),
            Value = result.Definition,
            Example = result.Examples.FirstOrDefault()
        }),
        Frequency = details.Frequency
    };
}
