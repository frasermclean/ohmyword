using FluentValidation;
using OhMyWord.Core.Requests.Words;

namespace OhMyWord.Core.Validators.Words;

public class DeleteWordRequestValidator : AbstractValidator<DeleteWordRequest>
{
    public DeleteWordRequestValidator()
    {
        RuleFor(request => request.Id).NotEmpty();
        RuleFor(request => request.PartOfSpeech).NotEmpty();
    }
}