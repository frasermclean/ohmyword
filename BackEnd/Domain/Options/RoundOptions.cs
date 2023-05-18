using System.ComponentModel.DataAnnotations;

namespace OhMyWord.Domain.Options;

public class RoundOptions
{
    public const string SectionName = "Round";
    public const double LetterHintDelayDefault = 3;
    private const double PostRoundDelayDefault = 5;
    public const int GuessLimitDefault = 3;

    [Range(1, 10)] public double LetterHintDelay { get; init; } = LetterHintDelayDefault;
    [Range(1, 60)] public double PostRoundDelay { get; init; } = PostRoundDelayDefault;
    [Range(1, 10)] public int GuessLimit { get; init; } = GuessLimitDefault;
}
