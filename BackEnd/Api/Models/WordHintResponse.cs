using OhMyWord.Core.Models;

namespace OhMyWord.Api.Models;

public class WordHintResponse
{
    public required int Length { get; init; }
    public required string Definition { get; init; }
    public required PartOfSpeech PartOfSpeech { get; init; }
    public required IEnumerable<LetterHintResponse> LetterHints { get; init; }

    public static WordHintResponse FromWordHint(WordHint wordHint) => new()
    {
        Length = wordHint.Length,
        Definition = wordHint.Definition.Value,
        PartOfSpeech = wordHint.Definition.PartOfSpeech,
        LetterHints = wordHint.LetterHints.Select(LetterHintResponse.FromLetterHint)
    };
}
