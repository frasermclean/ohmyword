using FluentValidation;
using OhMyWord.Core.Requests.Words;

namespace OhMyWord.Core.Validators.Words;

public class GetWordRequestValidator : AbstractValidator<GetWordRequest>
{
    public GetWordRequestValidator()
    {
        RuleFor(request => request.Id).NotEmpty();
        RuleFor(request => request.PartOfSpeech).NotEmpty();
    }
}