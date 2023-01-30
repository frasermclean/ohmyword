using OhMyWord.Core.Models;

namespace OhMyWord.Api.Endpoints.Words.Update;

public class UpdateWordRequest
{
    public string WordId { get; init; } = string.Empty;
    public List<Definition> Definitions { get; init; } = new();
}
