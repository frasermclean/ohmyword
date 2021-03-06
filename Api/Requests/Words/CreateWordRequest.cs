using OhMyWord.Core.Models;
using System.ComponentModel.DataAnnotations;

namespace OhMyWord.Api.Requests.Words;

public class CreateWordRequest
{
    [Required]
    public string Value { get; init; } = string.Empty;

    [Required]
    public string Definition { get; init; } = string.Empty;

    [Required]
    public PartOfSpeech? PartOfSpeech { get; init; }

    public Word ToWord() => new()
    {
        Id = Value,
        Definition = Definition,
        PartOfSpeech = PartOfSpeech.GetValueOrDefault()
    };
}
