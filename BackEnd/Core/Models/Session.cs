namespace OhMyWord.Core.Models;

public sealed record Session : Entity
{
    public int RoundCount { get; set; }
    public DateTime StartDate { get; init; } = DateTime.UtcNow;
    public DateTime? EndDate { get; set; }


    public static readonly Session Default = new()
    {
        Id = InvalidId, StartDate = DateTime.MinValue, LastModifiedTime = DateTime.MinValue
    };
}
