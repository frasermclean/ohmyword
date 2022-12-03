using MediatR;
using OhMyWord.Core.Responses.Words;
using OhMyWord.Data.Models;

namespace OhMyWord.Core.Requests.Words;

public class CreateWordRequest : IRequest<WordResponse>
{
    public string Value { get; init; } = string.Empty;
    public string Definition { get; init; } = string.Empty;
    public PartOfSpeech? PartOfSpeech { get; init; }

    public Word ToWord() => new()
    {
        Value = Value,
        Definition = Definition,
        PartOfSpeech = PartOfSpeech.GetValueOrDefault()
    };
}
