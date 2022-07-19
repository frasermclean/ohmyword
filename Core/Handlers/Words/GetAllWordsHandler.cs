using AutoMapper;
using MediatR;
using OhMyWord.Core.Requests.Words;
using OhMyWord.Core.Responses.Words;
using OhMyWord.Data.Services;

namespace OhMyWord.Core.Handlers.Words;

public class GetAllWordsHandler : IRequestHandler<GetAllWordsRequest, IEnumerable<WordResponse>>
{
    private readonly IWordsRepository wordsRepository;
    private readonly IMapper mapper;

    public GetAllWordsHandler(IWordsRepository wordsRepository, IMapper mapper)
    {
        this.wordsRepository = wordsRepository;
        this.mapper = mapper;
    }

    public async Task<IEnumerable<WordResponse>> Handle(GetAllWordsRequest request, CancellationToken cancellationToken)
    {
        var words = await wordsRepository.GetAllWordsAsync(cancellationToken);
        return mapper.Map<IEnumerable<WordResponse>>(words);
    }
}