using OhMyWord.Data.Models;
using OhMyWord.Data.Services;

namespace OhMyWord.Core.Responses.Words;

public class GetWordsResponse
{
    public int Offset { get; init; }
    public int Limit { get; init; }
    public int Total { get; init; }
    public string Filter { get; init; } = String.Empty;
    public GetWordsOrderBy OrderBy { get; init; }
    public SortDirection Direction { get; init; }
    public IEnumerable<WordResponse> Words { get; init; } = Enumerable.Empty<WordResponse>();
}
