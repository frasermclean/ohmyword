using System.ComponentModel.DataAnnotations;
using OhMyWord.Data.Models;

namespace OhMyWord.Core.Requests.Words;

public class UpdateWordRequest
{
    [Required]
    public string Value { get; init; } = string.Empty;
    
    [Required]
    public string Definition { get; init; } = string.Empty;

    public Word ToWord(PartOfSpeech partOfSpeech, Guid id) => new()
    {
        Id = id,
        Value = Value,
        Definition = Definition,
        PartOfSpeech = partOfSpeech,
    };
}
