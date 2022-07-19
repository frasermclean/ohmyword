using AutoMapper;
using MediatR;
using OhMyWord.Core.Requests.Words;
using OhMyWord.Core.Responses.Words;
using OhMyWord.Data.Services;

namespace OhMyWord.Core.Handlers.Words;

public class CreateWordHandler : IRequestHandler<CreateWordRequest, WordResponse>
{
    private readonly IWordsRepository wordsRepository;
    private readonly IMapper mapper;

    public CreateWordHandler(IWordsRepository wordsRepository, IMapper mapper)
    {
        this.wordsRepository = wordsRepository;
        this.mapper = mapper;
    }

    public async Task<WordResponse> Handle(CreateWordRequest request, CancellationToken cancellationToken)
    {
        var word = await wordsRepository.CreateWordAsync(request.ToWord());
        return mapper.Map<WordResponse>(word);
    }
}