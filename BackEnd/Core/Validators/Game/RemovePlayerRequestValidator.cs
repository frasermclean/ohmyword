using FluentValidation;
using OhMyWord.Core.Requests.Game;

namespace OhMyWord.Core.Validators.Game;

public class RemoveVisitorRequestValidator : AbstractValidator<RemoveVisitorRequest>
{
    public RemoveVisitorRequestValidator()
    {
        RuleFor(request => request.ConnectionId).NotEmpty();
    }
}
