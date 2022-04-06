using System.ComponentModel.DataAnnotations;

namespace OhMyWord.Services.Options;

public class GameServiceOptions
{
    public const string SectionName = "Game";

    [Range(5, 120, ErrorMessage = "Invalid round length.")]
    public int RoundLength { get; set; }

    [Range(5, 60, ErrorMessage = "Invalid next round delay.")]
    public int PostRoundDelay { get; set; }
}
