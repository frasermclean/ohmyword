namespace OhMyWord.Domain.Models;

public readonly struct RoundStartData
{
    public required int RoundNumber { get; init; }
    public required Guid RoundId { get; init; }
    public required WordHint WordHint { get; init; }
    public required DateTime StartDate { get; init; }
    public required DateTime EndDate { get; init; }    
}
