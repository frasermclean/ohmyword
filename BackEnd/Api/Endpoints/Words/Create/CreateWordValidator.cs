using FluentValidation;
using OhMyWord.Core.Models;

namespace OhMyWord.Api.Endpoints.Words.Create;

public class CreateWordValidator : Validator<CreateWordRequest>
{
    public CreateWordValidator()
    {
        RuleFor(request => request.Id)
            .NotEmpty()
            .Length(Word.MinLength, Word.MaxLength)
            .Must(id => id.All(Char.IsLetter))
            .WithMessage("Only single words with no spaces are allowed.");

        RuleFor(request => request.Definitions)
            .NotEmpty();

        RuleFor(request => request.Frequency)
            .InclusiveBetween(Word.FrequencyMinValue, Word.FrequencyMaxValue);
    }
}
