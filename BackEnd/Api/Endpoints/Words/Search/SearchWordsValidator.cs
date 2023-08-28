using FluentValidation;
using OhMyWord.Core.Services;
using OhMyWord.Data.CosmosDb.Services;

namespace OhMyWord.Api.Endpoints.Words.Search;

public class SearchWordsValidator : Validator<SearchWordsRequest>
{
    public SearchWordsValidator()
    {
        RuleFor(request => request.Offset).GreaterThanOrEqualTo(IWordsRepository.OffsetMinimum);
        RuleFor(request => request.Limit)
            .InclusiveBetween(IWordsRepository.LimitMinimum, IWordsRepository.LimitMaximum);
        RuleFor(request => request.OrderBy)
            .Must(orderBy => string.IsNullOrEmpty(orderBy) || WordsRepository.ValidOrderByValues.Contains(orderBy))
            .WithMessage(
                $"OrderBy must be one of the following values: {string.Join(", ", WordsRepository.ValidOrderByValues)}");
    }
}
