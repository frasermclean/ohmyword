namespace OhMyWord.Infrastructure.Models.Entities;

public record RoundPlayerData(Guid PlayerId)
{
    public int GuessCount { get; set; }
    public TimeSpan GuessTime { get; set; }
    public int PointsAwarded { get; set; }
};
