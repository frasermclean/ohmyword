using OhMyWord.Core.Models;

namespace OhMyWord.Api.Endpoints.Words.Create;

public class CreateWordRequest
{
    public string Id { get; init; } = string.Empty;
    public IEnumerable<Definition> Definitions { get; init; } = Enumerable.Empty<Definition>();
    public double Frequency { get; init; }
}
