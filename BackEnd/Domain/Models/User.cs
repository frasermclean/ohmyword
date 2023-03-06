using OhMyWord.Infrastructure.Entities;

namespace OhMyWord.Domain.Models;

public class User
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required string Email { get; init; }
    public required Role Role { get; init; }
}
