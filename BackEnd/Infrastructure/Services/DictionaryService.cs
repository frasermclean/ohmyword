namespace OhMyWord.Infrastructure.Services;

public interface IDictionaryService
{
    Task<string> GetDefinitionAsync(string word, CancellationToken cancellationToken = default);
}

public class DictionaryService : IDictionaryService
{
    private readonly HttpClient httpClient;

    public DictionaryService(HttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    public async Task<string> GetDefinitionAsync(string word, CancellationToken cancellationToken)
    {
        var response = await httpClient.GetAsync(word, cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStringAsync(cancellationToken);
    }
}
