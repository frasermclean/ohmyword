using OhMyWord.Data.Models;

namespace OhMyWord.Api.Responses.Words;

public class CurrentWordResponse
{
    public Word Word { get; init; } = default!;
    public DateTime Expiry { get; init; }
}
