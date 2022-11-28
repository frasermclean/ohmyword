using FluentValidation;
using OhMyWord.Core.Requests.Game;

namespace OhMyWord.Core.Validators.Game;

public class SubmitGuessRequestValidator : AbstractValidator<SubmitGuessRequest>
{
    public SubmitGuessRequestValidator()
    {
        RuleFor(request => request.RoundId).NotEmpty();
        RuleFor(request => request.Value).NotEmpty();
        RuleFor(request => request.ConnectionId).NotEmpty();
    }
}