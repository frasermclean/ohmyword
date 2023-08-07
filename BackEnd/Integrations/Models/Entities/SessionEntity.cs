namespace OhMyWord.Infrastructure.Models.Entities;

public record SessionEntity : Entity
{
    public int RoundCount { get; init; }
    public DateTime StartDate { get; init; } 
    public DateTime EndDate { get; init; }
}
