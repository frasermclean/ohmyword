using Microsoft.Extensions.Logging;
using OhMyWord.Integrations.RapidApi.Models.WordsApi;
using System.Net;
using System.Net.Http.Json;

namespace OhMyWord.Integrations.RapidApi.Services;

public interface IWordsApiClient
{
    /// <summary>
    /// Reach out to the WordsApi to look up a word.
    /// </summary>
    /// <param name="word">The word to search for</param>
    /// <param name="cancellationToken">Operation cancellation token</param>
    /// <returns>A populated <see cref="HttpRequestException"/> object if the word was found, null if not found.</returns>
    /// <exception cref="WordDetails">Any error except for not found.</exception>
    Task<WordDetails?> GetWordDetailsAsync(string word, CancellationToken cancellationToken = default);

    Task<WordDetails> GetRandomWordDetailsAsync(CancellationToken cancellationToken = default);
}

public sealed class WordsApiClient : IWordsApiClient
{
    private readonly ILogger<WordsApiClient> logger;
    private readonly HttpClient httpClient;

    public WordsApiClient(ILogger<WordsApiClient> logger, HttpClient httpClient)
    {
        this.logger = logger;
        this.httpClient = httpClient;
    }

    public Task<WordDetails?> GetWordDetailsAsync(string word, CancellationToken cancellationToken)
        => SendRequestAsync(new Uri(word, UriKind.Relative), cancellationToken);

    public async Task<WordDetails> GetRandomWordDetailsAsync(CancellationToken cancellationToken = default)
        => await SendRequestAsync(new Uri("?random=true", UriKind.Relative), cancellationToken) ??
           throw new InvalidOperationException("Random word return null reference");

    private async Task<WordDetails?> SendRequestAsync(Uri uri, CancellationToken cancellationToken)
    {
        var message = await httpClient.GetAsync(uri, cancellationToken);

        if (message.StatusCode == HttpStatusCode.NotFound)
        {
            logger.LogWarning("Found no details for word: {Uri}", uri);
            return default;
        }

        message.EnsureSuccessStatusCode();

        return await message.Content.ReadFromJsonAsync<WordDetails>(cancellationToken: cancellationToken);
    }
}
