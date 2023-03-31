namespace OhMyWord.Infrastructure.Options;

public class DictionaryOptions
{
    public const string SectionName = "Dictionary";
    public const string ApiBaseUrl = "https://www.dictionaryapi.com/api/v3/references/sd3/json";

    public string ApiKey { get; init; } = string.Empty;
}
