using FluentValidation;
using OhMyWord.Core.Requests.Game;

namespace OhMyWord.Core.Validators.Game;

public class RegisterPlayerRequestValidator : AbstractValidator<RegisterPlayerRequest>
{
    public RegisterPlayerRequestValidator()
    {
        RuleFor(request => request.ConnectionId).NotEmpty();
        RuleFor(request => request.VisitorId).NotEmpty();
    }
}