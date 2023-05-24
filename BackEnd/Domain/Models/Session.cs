namespace OhMyWord.Domain.Models;

public class Session
{
    public Guid Id { get; private init; } = Guid.NewGuid();
    public int RoundCount { get; set; }
    public DateTime StartDate { get; } = DateTime.UtcNow;
    public DateTime EndDate { get; set; }

    public static Session Default => new() { Id = Guid.Empty };
}
