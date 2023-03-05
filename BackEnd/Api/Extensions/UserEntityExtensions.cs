using OhMyWord.Data.Entities;
using User = OhMyWord.Api.Models.User;

namespace OhMyWord.Api.Extensions;

public static class UserEntityExtensions
{
    public static User ToUser(this UserEntity entity) => new()
    {
        Id = entity.Id, Name = entity.Name, Email = entity.Email, Role = entity.Role
    };
}
