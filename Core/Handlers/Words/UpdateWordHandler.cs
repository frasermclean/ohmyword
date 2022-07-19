using AutoMapper;
using FluentValidation;
using MediatR;
using OhMyWord.Core.Requests.Words;
using OhMyWord.Core.Responses.Words;
using OhMyWord.Data.Services;

namespace OhMyWord.Core.Handlers.Words;

public class UpdateWordHandler : IRequestHandler<UpdateWordRequest, WordResponse>
{
    private readonly IWordsRepository wordsRepository;
    private readonly IMapper mapper;
    private readonly IValidator<UpdateWordRequest> validator;

    public UpdateWordHandler(IWordsRepository wordsRepository, IMapper mapper, IValidator<UpdateWordRequest> validator)
    {
        this.wordsRepository = wordsRepository;
        this.mapper = mapper;
        this.validator = validator;
    }

    public async Task<WordResponse> Handle(UpdateWordRequest request, CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);
        var word = await wordsRepository.UpdateWordAsync(request.ToWord());
        return mapper.Map<WordResponse>(word);
    }
}