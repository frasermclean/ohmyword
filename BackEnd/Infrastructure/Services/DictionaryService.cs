using Microsoft.Extensions.Options;
using OhMyWord.Infrastructure.Entities.Dictionary;
using OhMyWord.Infrastructure.Options;
using System.Net.Http.Json;

namespace OhMyWord.Infrastructure.Services;

public interface IDictionaryService
{
    /// <summary>
    /// Reach out to the Merriam-Webster Dictionary API to look up a word.
    /// </summary>
    /// <param name="word">The word to search for</param>
    /// <param name="cancellationToken">Operation cancellation token</param>
    /// <returns>A collection of any matching <see cref="DictionaryWord"/> results.</returns>
    Task<IEnumerable<DictionaryWord>> LookupWordAsync(string word, CancellationToken cancellationToken = default);
}

public class DictionaryService : IDictionaryService
{
    private readonly HttpClient httpClient;
    private readonly string apiKey;

    public DictionaryService(HttpClient httpClient, IOptions<DictionaryOptions> options)
    {
        this.httpClient = httpClient;
        apiKey = options.Value.ApiKey;
    }

    public async Task<IEnumerable<DictionaryWord>> LookupWordAsync(string word, CancellationToken cancellationToken)
    {
        var uri = new Uri($"{word}?key={apiKey}", UriKind.Relative);
        var words = await httpClient.GetFromJsonAsync<IEnumerable<DictionaryWord>>(uri, cancellationToken);

        return words ?? Enumerable.Empty<DictionaryWord>();
    }
}
