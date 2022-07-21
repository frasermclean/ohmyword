using AutoMapper;
using MediatR;
using OhMyWord.Core.Requests.Words;
using OhMyWord.Core.Responses.Words;
using OhMyWord.Data.Services;

namespace OhMyWord.Core.Handlers.Words;

public class GetWordsHandler : IRequestHandler<GetWordsRequest, IEnumerable<WordResponse>>
{
    private readonly IWordsRepository wordsRepository;
    private readonly IMapper mapper;

    public GetWordsHandler(IWordsRepository wordsRepository, IMapper mapper)
    {
        this.wordsRepository = wordsRepository;
        this.mapper = mapper;
    }

    public async Task<IEnumerable<WordResponse>> Handle(GetWordsRequest request, CancellationToken cancellationToken)
    {
        var words = await wordsRepository.GetWordsAsync(request.Offset, request.Limit, cancellationToken);
        return mapper.Map<IEnumerable<WordResponse>>(words);
    }
}
