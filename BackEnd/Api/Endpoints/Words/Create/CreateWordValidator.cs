using FluentValidation;

namespace OhMyWord.Api.Endpoints.Words.Create;

public class CreateWordValidator : Validator<CreateWordRequest>
{
    public CreateWordValidator()
    {
        RuleFor(request => request.Id)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .Length(3, 16)
            .Must(id => id.All(Char.IsLetter))
            .WithMessage("Only single words with no spaces are allowed.");
    }
}
