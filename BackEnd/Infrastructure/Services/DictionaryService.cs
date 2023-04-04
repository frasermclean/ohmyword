using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OhMyWord.Infrastructure.Entities.Dictionary;
using OhMyWord.Infrastructure.Options;
using System.Net.Http.Json;
using System.Text.Json;

namespace OhMyWord.Infrastructure.Services;

public interface IDictionaryService
{
    /// <summary>
    /// Reach out to the Merriam-Webster Dictionary API to look up a word.
    /// </summary>
    /// <param name="wordId">The word to search for</param>
    /// <param name="cancellationToken">Operation cancellation token</param>
    /// <returns>A collection of any matching <see cref="DictionaryWord"/> results.</returns>
    Task<IEnumerable<DictionaryWord>> LookupWordAsync(string wordId, CancellationToken cancellationToken = default);
}

public class DictionaryService : IDictionaryService
{
    private readonly ILogger<DictionaryService> logger;
    private readonly HttpClient httpClient;
    private readonly string apiKey;

    public DictionaryService(ILogger<DictionaryService> logger, HttpClient httpClient,
        IOptions<DictionaryOptions> options)
    {
        this.logger = logger;
        this.httpClient = httpClient;
        apiKey = options.Value.ApiKey;
    }

    public async Task<IEnumerable<DictionaryWord>> LookupWordAsync(string wordId, CancellationToken cancellationToken)
    {
        try
        {
            var uri = new Uri($"{wordId}?key={apiKey}", UriKind.Relative);
            var words = await httpClient.GetFromJsonAsync<IEnumerable<DictionaryWord>>(uri, cancellationToken);
            logger.LogInformation("Found {Count} definitions for word: {Word}", words?.Count() ?? 0, wordId);
            return words ?? Enumerable.Empty<DictionaryWord>();
        }
        catch (JsonException exception)
        {
            logger.LogError(exception, "Error deserializing dictionary API response");
            return Enumerable.Empty<DictionaryWord>();
        }
    }
}
