using OhMyWord.Infrastructure.Entities;
using User = OhMyWord.Domain.Models.User;

namespace OhMyWord.Domain.Extensions;

public static class UserEntityExtensions
{
    public static User ToUser(this UserEntity entity) => new()
    {
        Id = entity.PartitionKey, Name = entity.Name, Email = entity.Email, Role = entity.Role
    };

    public static UserEntity ToEntity(this User user) => new()
    {
        PartitionKey = user.Id, Name = user.Name, Email = user.Email, Role = user.Role
    };
}
