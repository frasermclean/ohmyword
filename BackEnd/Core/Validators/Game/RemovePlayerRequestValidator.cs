using FluentValidation;
using OhMyWord.Core.Requests.Game;

namespace OhMyWord.Core.Validators.Game;

public class RemovePlayerRequestValidator : AbstractValidator<RemovePlayerRequest>
{
    public RemovePlayerRequestValidator()
    {
        RuleFor(request => request.ConnectionId).NotEmpty();
    }
}