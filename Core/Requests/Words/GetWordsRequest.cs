using MediatR;
using OhMyWord.Core.Responses.Words;
using OhMyWord.Data.Services;

namespace OhMyWord.Core.Requests.Words;

public class GetWordsRequest : IRequest<GetWordsResponse>
{
    public int Offset { get; init; } = WordsRepository.OffsetMinimum;
    public int Limit { get; init; } = WordsRepository.LimitDefault;
    public string? Filter { get; init; }
}
