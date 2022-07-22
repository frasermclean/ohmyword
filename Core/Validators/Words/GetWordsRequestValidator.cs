using FluentValidation;
using OhMyWord.Core.Requests.Words;
using OhMyWord.Data.Models;
using OhMyWord.Data.Services;
using System.Text.RegularExpressions;

namespace OhMyWord.Core.Validators.Words;

public class GetWordsRequestValidator : AbstractValidator<GetWordsRequest>
{
    public GetWordsRequestValidator()
    {
        RuleFor(request => request.Offset).GreaterThanOrEqualTo(WordsRepository.OffsetMinimum);
        RuleFor(request => request.Limit).InclusiveBetween(WordsRepository.LimitMinimum, WordsRepository.LimitMaximum);        
        RuleFor(request => request.OrderBy).IsInEnum();
    }
}
