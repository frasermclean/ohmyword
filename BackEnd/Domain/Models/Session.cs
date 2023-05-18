namespace OhMyWord.Domain.Models;

public class Session
{
    public Guid Id { get; init; } 
    public int RoundCount { get; set; }
    public DateTime StartDate { get; } = DateTime.UtcNow;
    public DateTime EndDate { get; set; }

    public static Session Default => new();
}
