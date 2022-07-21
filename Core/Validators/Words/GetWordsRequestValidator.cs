using FluentValidation;
using OhMyWord.Core.Requests.Words;
using OhMyWord.Data.Services;
using System.Text.RegularExpressions;

namespace OhMyWord.Core.Validators.Words;

public class GetWordsRequestValidator : AbstractValidator<GetWordsRequest>
{
    private static readonly Regex OrderByRegex =
        new("value|definition|partOfSpeech|lastModifiedTime", RegexOptions.Compiled);

    public GetWordsRequestValidator()
    {
        RuleFor(request => request.Offset).GreaterThanOrEqualTo(0);
        RuleFor(request => request.Limit).InclusiveBetween(WordsRepository.LimitMinimum, WordsRepository.LimitMaximum);
        RuleFor(request => request.OrderBy).Matches(OrderByRegex);
    }
}
