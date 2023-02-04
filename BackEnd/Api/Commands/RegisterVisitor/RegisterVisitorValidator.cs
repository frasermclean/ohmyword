using FastEndpoints;
using FluentValidation;

namespace OhMyWord.Api.Commands.RegisterVisitor;

public class RegisterVisitorValidator : Validator<RegisterVisitorCommand>
{
    public RegisterVisitorValidator()
    {
        RuleFor(command => command.ConnectionId).NotEmpty();
        RuleFor(command => command.VisitorId).NotEmpty();
    }
}
