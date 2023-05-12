using OhMyWord.Infrastructure.Models;

namespace OhMyWord.Api.Endpoints.Definitions.Get;

public class GetDefinitionsRequest
{
    public string WordId { get; init; } = string.Empty;
    public PartOfSpeech? PartOfSpeech { get; init; }
}
