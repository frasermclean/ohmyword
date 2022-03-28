using OhMyWord.Data.Models;
using System.ComponentModel.DataAnnotations;

namespace OhMyWord.Api.Requests.Words;

public class CreateWordRequest
{
    [Required]
    public string? Value { get; set; }

    [Required]
    public string? Definition { get; set; }

    public Word ToWord() => new()
    {
        Value = Value ?? throw new ArgumentNullException(nameof(Value)),
        Definition = Definition ?? throw new ArgumentNullException(nameof(Definition))
    };
}
