using OhMyWord.Core.Models;
using System.Text.Json.Serialization;

namespace OhMyWord.Api.Models;

public class WordResponse
{
    public required string Id { get; init; }
    public required int Length { get; init; }
    public required IEnumerable<Definition> Definitions { get; init; }
    public required double Frequency { get; init; }
    public required int Bounty { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public required Guid? LastModifiedBy { get; init; }

    public required DateTime LastModifiedTime { get; init; }

    public static WordResponse FromWord(Word word) => new()
    {
        Id = word.Id,
        Length = word.Length,
        Definitions = word.Definitions,
        Frequency = word.Frequency,
        Bounty = word.Bounty,
        LastModifiedBy = word.LastModifiedBy,
        LastModifiedTime = word.LastModifiedTime
    };
}
