using MediatR;
using OhMyWord.Core.Responses.Words;
using OhMyWord.Data.Models;

namespace OhMyWord.Core.Requests.Words;

public class UpdateWordRequest : IRequest<WordResponse>
{
    public Guid? Id { get; init; }
    public string? Value { get; init; }
    public string? Definition { get; init; }
    public PartOfSpeech? PartOfSpeech { get; init; }

    public Word ToWord() => new()
    {
        Id = Id ?? default,
        Value = Value ?? string.Empty,
        Definition = Definition ?? string.Empty,
        PartOfSpeech = PartOfSpeech ?? default,
    };
}