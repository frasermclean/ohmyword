namespace OhMyWord.Data.Entities;

public record UserEntity : Entity
{
    public string Name { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public Role Role { get; init; } = Role.User;
}
