namespace OhMyWord.Domain.Models;

public record RoundPlayerData
{
    public string PlayerId { get; }
    public int GuessCount { get; set; }
    public TimeSpan GuessTime { get; set; }
    public int PointsAwarded { get; set; }

    internal RoundPlayerData(string playerId)
    {
        PlayerId = playerId;
    }
};
