namespace OhMyWord.Api.Models;

public class GuessProcessedResult
{    
    public bool IsCorrect { get; init; }
    public int PointsAwarded { get; init; }

    public static readonly GuessProcessedResult Default = new();
}
