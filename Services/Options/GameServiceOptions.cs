using System.ComponentModel.DataAnnotations;

namespace OhMyWord.Services.Options;

public class GameServiceOptions
{
    public const string SectionName = "Game";

    [Range(1d, 20d, ErrorMessage = "Invalid letter hint delay.")]
    public double LetterHintDelay { get; set; }

    [Range(1d, 60d, ErrorMessage = "Invalid post round delay.")]
    public double PostRoundDelay { get; set; }
}
