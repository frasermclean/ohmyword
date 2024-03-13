namespace OhMyWord.Core.Models;

public record struct LetterHint(int Position, char Value)
{
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
}
