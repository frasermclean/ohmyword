using FluentValidation;
using OhMyWord.Core.Requests.Words;
using OhMyWord.Data.Services;

namespace OhMyWord.Core.Validators.Words;

public class GetWordsRequestValidator : AbstractValidator<GetWordsRequest>
{
    public GetWordsRequestValidator()
    {
        RuleFor(request => request.Offset).GreaterThanOrEqualTo(0);
        RuleFor(request => request.Limit).InclusiveBetween(WordsRepository.LimitMinimum, WordsRepository.LimitMaximum);
    }
}
