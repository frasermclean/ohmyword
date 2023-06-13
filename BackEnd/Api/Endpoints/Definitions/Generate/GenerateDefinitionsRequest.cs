using OhMyWord.Infrastructure.Models.Entities;

namespace OhMyWord.Api.Endpoints.Definitions.Get;

public class GenerateDefinitionsRequest
{
    public string WordId { get; init; } = string.Empty;
    public PartOfSpeech? PartOfSpeech { get; init; }
}
