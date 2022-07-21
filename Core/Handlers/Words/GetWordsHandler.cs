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
        var wordsTask = wordsRepository.GetWordsAsync(request.Offset, request.Limit, request.Filter, request.OrderBy,
            request.Desc, cancellationToken);
        var totalTask = wordsRepository.GetWordCountAsync(cancellationToken);

        await Task.WhenAll(wordsTask, totalTask);

        return new GetWordsResponse
        {
            Offset = request.Offset,
            Limit = request.Limit,
            Total = totalTask.Result,
            Filter = request.Filter,
            OrderBy = request.OrderBy,
            Desc = request.Desc,
            Words = mapper.Map<IEnumerable<WordResponse>>(wordsTask.Result)
        };
    }
}
