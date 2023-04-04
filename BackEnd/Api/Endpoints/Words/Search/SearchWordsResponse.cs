using OhMyWord.Domain.Models;

namespace OhMyWord.Api.Endpoints.Words.Search;

public class SearchWordsResponse
{
    public int Total { get; init; }
    public IEnumerable<Word> Words { get; init; } = Enumerable.Empty<Word>();
}
