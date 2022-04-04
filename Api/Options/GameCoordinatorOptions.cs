using System.ComponentModel.DataAnnotations;

namespace OhMyWord.Api.Options;

public class GameCoordinatorOptions
{
    public const string SectionName = "GameCoordinator";

    [Range(5, 120, ErrorMessage = "Invalid round length.")]
    public int RoundLength { get; set; }

    [Range(5, 60, ErrorMessage = "Invalid next round delay.")]
    public int NextRoundDelay { get; set; }
}
