using AutoMapper;
using MediatR;
using OhMyWord.Core.Requests.Words;
using OhMyWord.Core.Responses.Words;
using OhMyWord.Data.Services;

namespace OhMyWord.Core.Handlers.Words;

public class UpdateWordHandler : IRequestHandler<UpdateWordRequest, WordResponse>
{
    private readonly IWordsRepository wordsRepository;
    private readonly IMapper mapper;

    public UpdateWordHandler(IWordsRepository wordsRepository, IMapper mapper)
    {
        this.wordsRepository = wordsRepository;
        this.mapper = mapper;
    }

    public async Task<WordResponse> Handle(UpdateWordRequest request, CancellationToken cancellationToken)
    {
        var word = await wordsRepository.UpdateWordAsync(request.ToWord());
        return mapper.Map<WordResponse>(word);
    }
}