using OhMyWord.Core.Models;

namespace OhMyWord.Api.Models;

public struct LetterHintResponse
{
    public required int Position { get; init; }
    public required char Value { get; init; }

    public static LetterHintResponse FromLetterHint(LetterHint letterHint) => new()
    {
        Position = letterHint.Position, Value = letterHint.Value
    };
}
