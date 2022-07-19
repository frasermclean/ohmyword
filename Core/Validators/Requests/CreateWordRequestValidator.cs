using FluentValidation;
using OhMyWord.Core.Requests.Words;

namespace OhMyWord.Core.Validators.Requests;

public class CreateWordRequestValidator : AbstractValidator<CreateWordRequest>
{
    public CreateWordRequestValidator()
    {
        RuleFor(request => request.Value).NotEmpty();
        RuleFor(request => request.Definition).NotEmpty();
        RuleFor(request => request.PartOfSpeech).NotNull();
    }
}