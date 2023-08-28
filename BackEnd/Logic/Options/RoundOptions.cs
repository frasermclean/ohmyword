using System.ComponentModel.DataAnnotations;

namespace OhMyWord.Logic.Options;

public class RoundOptions
{
    public const string SectionName = "Round";

    [Range(1, 10)] public double LetterHintDelay { get; init; } = 3;
    [Range(1, 60)] public double PostRoundDelay { get; init; } = 5;
    [Range(1, 10)] public int GuessLimit { get; init; } = 3;
}
