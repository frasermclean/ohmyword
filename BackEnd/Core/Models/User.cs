namespace OhMyWord.Core.Models;

public record User
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required string Email { get; init; }
    public required string IdentityProvider { get; init; }
    public required Role Role { get; init; }
}
