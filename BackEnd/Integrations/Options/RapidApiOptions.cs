using System.ComponentModel.DataAnnotations;

namespace OhMyWord.Infrastructure.Options;

public class RapidApiOptions
{
    public const string SectionName = "RapidApi";

    [Required] public string ApiKey { get; init; } = string.Empty;
}
