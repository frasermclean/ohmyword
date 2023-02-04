using FastEndpoints;
using FluentValidation;

namespace OhMyWord.Api.Endpoints.Words.Create;

public class CreateWordValidator : Validator<CreateWordRequest>
{
    public CreateWordValidator()
    {
        RuleFor(request => request.Id).NotEmpty();
        RuleFor(request => request.Definitions).NotEmpty();
    }
}
