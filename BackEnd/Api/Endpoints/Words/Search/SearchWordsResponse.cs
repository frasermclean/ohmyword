using OhMyWord.Api.Models;

namespace OhMyWord.Api.Endpoints.Words.Search;

public class SearchWordsResponse
{
    public int Total { get; init; }
    public IEnumerable<WordResponse> Words { get; init; } = Enumerable.Empty<WordResponse>();
}
