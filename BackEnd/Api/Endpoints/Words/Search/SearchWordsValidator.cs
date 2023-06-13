using FluentValidation;
using OhMyWord.Infrastructure.Services.Repositories;

namespace OhMyWord.Api.Endpoints.Words.Search;

public class SearchWordsValidator : Validator<SearchWordsRequest>
{
    public SearchWordsValidator()
    {
        RuleFor(request => request.Offset).GreaterThanOrEqualTo(WordsRepository.OffsetMinimum);
        RuleFor(request => request.Limit).InclusiveBetween(WordsRepository.LimitMinimum, WordsRepository.LimitMaximum);
        RuleFor(request => request.Direction).IsInEnum();
        RuleFor(request => request.OrderBy).IsInEnum();
    }
}
