using OhMyWord.Core.Models;

namespace OhMyWord.Api.Endpoints.Definitions.Generate;

public class GenerateDefinitionsRequest
{
    public string WordId { get; init; } = string.Empty;
    public PartOfSpeech? PartOfSpeech { get; init; }
}
