namespace OhMyWord.Domain.Models;

public sealed class Session : IDisposable
{
    public Guid Id { get; private init; } = Guid.NewGuid();
    public int RoundCount { get; set; }
    public DateTime StartDate { get; } = DateTime.UtcNow;
    public DateTime EndDate { get; private set; }

    public bool IsActive => StartDate > DateTime.UtcNow && EndDate == default;

    public void Dispose()
    {
        EndDate = DateTime.UtcNow;
    }    
}
