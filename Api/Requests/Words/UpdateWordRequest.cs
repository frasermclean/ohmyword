using OhMyWord.Core.Models;
using System.ComponentModel.DataAnnotations;

namespace OhMyWord.Api.Requests.Words;

public class UpdateWordRequest
{
    [Required]
    public string Definition { get; init; } = string.Empty;

    public Word ToWord(PartOfSpeech partOfSpeech, string value) => new()
    {
        Id = value,
        Definition = Definition,
        PartOfSpeech = partOfSpeech,
    };
}
