using OhMyWord.Core.Models;

namespace OhMyWord.Data.CosmosDb.Models;

public sealed record WordItem : UserCreatedItem
{
    public IEnumerable<Definition> Definitions { get; init; } = Enumerable.Empty<Definition>();
    public double Frequency { get; init; }
}
