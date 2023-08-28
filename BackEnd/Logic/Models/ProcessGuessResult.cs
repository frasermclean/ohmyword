namespace OhMyWord.Logic.Models;

public class ProcessGuessResult
{
    public bool IsCorrect { get; init; }
    public int PointsAwarded { get; init; }
    public string Message { get; init; } = string.Empty;

    public static readonly ProcessGuessResult Default = new();
}
