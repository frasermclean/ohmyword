using AutoMapper;
using FluentValidation;
using MediatR;
using OhMyWord.Core.Requests.Words;
using OhMyWord.Core.Responses.Words;
using OhMyWord.Data.Models;
using OhMyWord.Data.Services;

namespace OhMyWord.Core.Handlers.Words;

public class GetWordHandler : IRequestHandler<GetWordRequest, WordResponse>
{
    private readonly IWordsRepository wordsRepository;
    private readonly IMapper mapper;
    private readonly IValidator<GetWordRequest> validator;

    public GetWordHandler(IWordsRepository wordsRepository, IMapper mapper, IValidator<GetWordRequest> validator)
    {
        this.wordsRepository = wordsRepository;
        this.mapper = mapper;
        this.validator = validator;
    }

    public async Task<WordResponse> Handle(GetWordRequest request, CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);
        var result = await wordsRepository.GetWordAsync(request.PartOfSpeech ?? default, request.Id ?? default);
        var word = result.Resource ?? Word.Default;
        return mapper.Map<WordResponse>(word);
    }
}