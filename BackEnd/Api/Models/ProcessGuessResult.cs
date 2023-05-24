namespace OhMyWord.Api.Models;

public class ProcessGuessResult
{    
    public bool IsCorrect { get; init; }
    public int PointsAwarded { get; init; }

    public static readonly ProcessGuessResult Default = new();
}
