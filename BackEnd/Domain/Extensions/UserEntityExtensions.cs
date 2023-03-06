using OhMyWord.Infrastructure.Entities;
using User = OhMyWord.Domain.Models.User;

namespace OhMyWord.Domain.Extensions;

public static class UserEntityExtensions
{
    public static User ToUser(this UserEntity entity) => new()
    {
        Id = entity.Id, Name = entity.Name, Email = entity.Email, Role = entity.Role
    };
}
