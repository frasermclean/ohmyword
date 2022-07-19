using AutoMapper;
using FluentValidation;
using MediatR;
using OhMyWord.Core.Requests.Words;
using OhMyWord.Core.Responses.Words;
using OhMyWord.Data.Models;
using OhMyWord.Data.Services;

namespace OhMyWord.Core.Handlers.Words;

public class CreateWordHandler : IRequestHandler<CreateWordRequest, WordResponse>
{
    private readonly IWordsRepository wordsRepository;
    private readonly IMapper mapper;
    private readonly IValidator<CreateWordRequest> validator;

    public CreateWordHandler(IWordsRepository wordsRepository, IMapper mapper, IValidator<CreateWordRequest> validator)
    {
        this.wordsRepository = wordsRepository;
        this.mapper = mapper;
        this.validator = validator;
    }

    public async Task<WordResponse> Handle(CreateWordRequest request, CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);
        var result = await wordsRepository.CreateWordAsync(request.ToWord());
        var word = result.Resource ?? Word.Default;
        return mapper.Map<WordResponse>(word);
    }
}