using System.ComponentModel.DataAnnotations;

namespace OhMyWord.Api.Options;

public class GameServiceOptions
{
    public const string SectionName = "Game";
    public const double LetterHintDelayDefault = 3;
    public const double PostRoundDelayDefault = 5;

    [Range(1d, 20d, ErrorMessage = "Invalid letter hint delay.")]
    public double LetterHintDelay { get; init; } = LetterHintDelayDefault;

    [Range(1d, 60d, ErrorMessage = "Invalid post round delay.")]
    public double PostRoundDelay { get; init; } = PostRoundDelayDefault;
}
