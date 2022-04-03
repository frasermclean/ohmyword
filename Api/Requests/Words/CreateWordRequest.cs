using OhMyWord.Data.Models;
using System.ComponentModel.DataAnnotations;

namespace OhMyWord.Api.Requests.Words;

public class CreateWordRequest
{
    [Required]
    public string Id { get; init; } = string.Empty;

    [Required]
    public string Definition { get; init; } = string.Empty;

    [Required]
    public PartOfSpeech? PartOfSpeech { get; init; } 

    public Word ToWord() => new()
    {
        Id = Id,
        Definition = Definition,
        PartOfSpeech = PartOfSpeech.GetValueOrDefault()
    };
}
