namespace OhMyWord.Core.Models;

public class RoundPlayerData
{
    public int GuessCount { get; set; }
    public TimeSpan GuessTime { get; set; }
    public int PointsAwarded { get; set; }
};
