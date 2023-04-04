using FluentValidation;
using OhMyWord.Domain.Models;

namespace OhMyWord.Api.Endpoints.Words.Create;

public class CreateWordValidator : Validator<CreateWordRequest>
{
    public CreateWordValidator()
    {
        RuleFor(request => request.Id)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .Length(Word.MinLength, Word.MaxLength)
            .Must(id => id.All(Char.IsLetter))
            .WithMessage("Only single words with no spaces are allowed.");

        RuleFor(request => request.Definitions)
            .NotEmpty();
    }
}
