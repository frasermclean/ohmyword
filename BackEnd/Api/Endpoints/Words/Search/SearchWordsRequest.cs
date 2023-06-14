using OhMyWord.Infrastructure.Services.Repositories;

namespace OhMyWord.Api.Endpoints.Words.Search;

public class SearchWordsRequest
{
    public int Offset { get; init; } = WordsRepository.OffsetMinimum;
    public int Limit { get; init; } = WordsRepository.LimitDefault;
    public string Filter { get; init; } = string.Empty;
    public string OrderBy { get; init; } = string.Empty;
    public bool IsDescending { get; init; }
}
