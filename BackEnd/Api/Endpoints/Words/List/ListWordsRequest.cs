using OhMyWord.Data.Services;

namespace OhMyWord.Api.Endpoints.Words.List;

public class ListWordsRequest
{
    public int Offset { get; init; } = WordsRepository.OffsetMinimum;
    public int Limit { get; init; } = WordsRepository.LimitDefault;
    public string Filter { get; init; } = string.Empty;
    public ListWordsOrderBy OrderBy { get; init; } = ListWordsOrderBy.Id;
    public SortDirection Direction { get; init; } = SortDirection.Ascending;
}
