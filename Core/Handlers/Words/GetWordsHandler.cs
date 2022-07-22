using AutoMapper;
using MediatR;
using OhMyWord.Core.Requests.Words;
using OhMyWord.Core.Responses.Words;
using OhMyWord.Data.Services;

namespace OhMyWord.Core.Handlers.Words;

public class GetWordsHandler : IRequestHandler<GetWordsRequest, GetWordsResponse>
{
    private readonly IWordsRepository wordsRepository;
    private readonly IMapper mapper;

    public GetWordsHandler(IWordsRepository wordsRepository, IMapper mapper)
    {
        this.wordsRepository = wordsRepository;
        this.mapper = mapper;
    }

    public async Task<GetWordsResponse> Handle(GetWordsRequest request, CancellationToken cancellationToken)
    {
        var (words, total) = await wordsRepository.GetWordsAsync(
            request.Offset, request.Limit, request.Filter ?? string.Empty,
            request.OrderBy, request.Desc, cancellationToken);

        return new GetWordsResponse
        {
            Offset = request.Offset,
            Limit = request.Limit,
            Total = total,
            Filter = request.Filter ?? string.Empty,
            OrderBy = request.OrderBy,
            Desc = request.Desc,
            Words = mapper.Map<IEnumerable<WordResponse>>(words)
        };
    }
}
