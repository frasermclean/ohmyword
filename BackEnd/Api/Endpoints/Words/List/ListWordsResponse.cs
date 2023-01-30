using OhMyWord.Core.Models;
using OhMyWord.Data.Services;

namespace OhMyWord.Api.Endpoints.Words.List;

public class ListWordsResponse
{
    public int Offset { get; init; }
    public int Limit { get; init; }
    public int Total { get; init; }
    public string Filter { get; init; } = String.Empty;
    public ListWordsOrderBy OrderBy { get; init; }
    public SortDirection Direction { get; init; }
    public IEnumerable<Word> Words { get; init; } = Enumerable.Empty<Word>();
}
