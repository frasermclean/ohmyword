using OhMyWord.Core.Models;

namespace OhMyWord.Api.Models;

public class WordResponse
{
    public required string Id { get; init; }
    public int Length => Id.Length;
    public required IEnumerable<Definition> Definitions { get; init; }
    public required double Frequency { get; init; }
    public required Guid? LastModifiedBy { get; init; }
    public required DateTime LastModifiedTime { get; init; }

    public static WordResponse FromWord(Word word) => new()
    {
        Id = word.Id,
        Definitions = word.Definitions,
        Frequency = word.Frequency,
        LastModifiedBy = word.LastModifiedBy,
        LastModifiedTime = word.LastModifiedTime
    };
}
