using AutoMapper;
using MediatR;
using OhMyWord.Core.Requests.Words;
using OhMyWord.Core.Responses.Words;
using OhMyWord.Data.Services;

namespace OhMyWord.Core.Handlers.Words;

public class GetWordHandler : IRequestHandler<GetWordRequest, WordResponse>
{
    private readonly IWordsRepository wordsRepository;
    private readonly IMapper mapper;

    public GetWordHandler(IWordsRepository wordsRepository, IMapper mapper)
    {
        this.wordsRepository = wordsRepository;
        this.mapper = mapper;
    }

    public async Task<WordResponse> Handle(GetWordRequest request, CancellationToken cancellationToken)
    {
        var word = await wordsRepository.GetWordAsync(request.PartOfSpeech.GetValueOrDefault(), request.Id.GetValueOrDefault());
        return mapper.Map<WordResponse>(word);
    }
}