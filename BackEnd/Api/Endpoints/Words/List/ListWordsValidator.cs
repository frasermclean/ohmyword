using FastEndpoints;
using FluentValidation;
using OhMyWord.Data.Services;

namespace OhMyWord.Api.Endpoints.Words.List;

public class ListWordsValidator : Validator<ListWordsRequest>
{
    public ListWordsValidator()
    {
        RuleFor(request => request.Offset).GreaterThanOrEqualTo(WordsRepository.OffsetMinimum);
        RuleFor(request => request.Limit).InclusiveBetween(WordsRepository.LimitMinimum, WordsRepository.LimitMaximum);
        RuleFor(request => request.Direction).IsInEnum();
        RuleFor(request => request.OrderBy).IsInEnum();
    }
}
