using System.ComponentModel.DataAnnotations;

namespace OhMyWord.WordsApi.Options;

public class WordsApiOptions
{
    public const string SectionName = "WordsApi";

    [Required] public string ApiKey { get; init; } = string.Empty;
}
