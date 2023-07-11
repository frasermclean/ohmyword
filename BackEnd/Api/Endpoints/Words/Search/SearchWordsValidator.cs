using FluentValidation;
using OhMyWord.Infrastructure.Services.Repositories;

namespace OhMyWord.Api.Endpoints.Words.Search;

public class SearchWordsValidator : Validator<SearchWordsRequest>
{
    public SearchWordsValidator()
    {
        RuleFor(request => request.Offset).GreaterThanOrEqualTo(WordsRepository.OffsetMinimum);
        RuleFor(request => request.Limit).InclusiveBetween(WordsRepository.LimitMinimum, WordsRepository.LimitMaximum);
        RuleFor(request => request.OrderBy)
            .Must(orderBy => string.IsNullOrEmpty(orderBy) || WordsRepository.ValidOrderByValues.Contains(orderBy))
            .WithMessage(
                $"OrderBy must be one of the following values: {string.Join(", ", WordsRepository.ValidOrderByValues)}");
    }
}
