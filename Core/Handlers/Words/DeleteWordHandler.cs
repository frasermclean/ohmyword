using AutoMapper;
using FluentValidation;
using MediatR;
using OhMyWord.Core.Requests.Words;
using OhMyWord.Core.Responses.Words;
using OhMyWord.Data.Models;
using OhMyWord.Data.Services;

namespace OhMyWord.Core.Handlers.Words;

public class DeleteWordHandler : AsyncRequestHandler<DeleteWordRequest>
{
    private readonly IWordsRepository wordsRepository;
    private readonly IValidator<DeleteWordRequest> validator;

    public DeleteWordHandler(IWordsRepository wordsRepository, IValidator<DeleteWordRequest> validator)
    {
        this.wordsRepository = wordsRepository;
        this.validator = validator;
    }

    protected override async Task Handle(DeleteWordRequest request, CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);
        await wordsRepository.DeleteWordAsync(request.PartOfSpeech.GetValueOrDefault(), request.Id.GetValueOrDefault());
    }
}