using FluentValidation;
using OhMyWord.Core.Requests.Words;

namespace OhMyWord.Core.Validators.Requests;

public class UpdateWordRequestValidator : AbstractValidator<UpdateWordRequest>
{
    public UpdateWordRequestValidator()
    {
        RuleFor(request => request.Id).NotEmpty();
        RuleFor(request => request.Value).NotEmpty();
        RuleFor(request => request.Definition).NotEmpty();
        RuleFor(request => request.PartOfSpeech).NotEmpty();
    }
}