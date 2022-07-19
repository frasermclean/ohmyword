using System.ComponentModel.DataAnnotations;
using OhMyWord.Data.Models;

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
        Value = Value,
        Definition = Definition,
        PartOfSpeech = PartOfSpeech.GetValueOrDefault()
    };
}
