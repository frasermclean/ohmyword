namespace OhMyWord.Core.Models;

public record struct Interval(DateTime StartDate, DateTime EndDate)
{
    public static readonly Interval Default = new(DateTime.MinValue, DateTime.MinValue);
}
