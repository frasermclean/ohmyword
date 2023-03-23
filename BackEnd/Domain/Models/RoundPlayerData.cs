namespace OhMyWord.Domain.Models;

public record RoundPlayerData
{
    public required string PlayerId { get; init; }
    public bool GuessedCorrectly { get; set; }
    public int GuessCount { get; set; }
    public int PointsAwarded { get; set; }
};
