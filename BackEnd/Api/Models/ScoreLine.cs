namespace OhMyWord.Api.Models;

public class ScoreLine
{
    public required string PlayerName { get; init; }
    public required string ConnectionId { get; init; }
    public required string CountryCode { get; init; }
    public required int PointsAwarded { get; set; }
    public required int GuessCount { get; set; }
}
