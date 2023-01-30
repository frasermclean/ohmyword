using FastEndpoints;
using FluentValidation;

namespace OhMyWord.Api.Commands.RegisterPlayer;

public class RegisterPlayerValidator : Validator<RegisterPlayerCommand>
{
    public RegisterPlayerValidator()
    {
        RuleFor(command => command.ConnectionId).NotEmpty();
        RuleFor(command => command.VisitorId).NotEmpty();
    }
}
