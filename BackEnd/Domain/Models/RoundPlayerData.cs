namespace OhMyWord.Domain.Models;

public record RoundPlayerData
{
    public int GuessCount { get; set; }
    public int PointsAwarded { get; set; }
};
