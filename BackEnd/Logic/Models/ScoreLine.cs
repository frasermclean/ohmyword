namespace OhMyWord.Logic.Models;

public class ScoreLine
{
    public required string PlayerName { get; init; }
    public required string ConnectionId { get; init; }
    public required string CountryName { get; init; }
    public required string CountryCode { get; init; }
    public required int PointsAwarded { get; set; }
    public required int GuessCount { get; set; }
    public required double GuessTimeMilliseconds { get; set; }
}
