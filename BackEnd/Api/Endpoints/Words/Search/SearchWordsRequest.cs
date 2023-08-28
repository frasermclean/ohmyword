using OhMyWord.Core.Services;

namespace OhMyWord.Api.Endpoints.Words.Search;

public class SearchWordsRequest
{
    public int Offset { get; init; } = IWordsRepository.OffsetMinimum;
    public int Limit { get; init; } = IWordsRepository.LimitDefault;
    public string Filter { get; init; } = string.Empty;
    public string OrderBy { get; init; } = string.Empty;
    public bool IsDescending { get; init; }
}
