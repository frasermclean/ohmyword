using OhMyWord.Domain.Models;
using OhMyWord.Infrastructure.Entities;

namespace OhMyWord.Domain.Extensions;

public static class VisitorEntityExtensions
{
    public static Visitor ToVisitor(this VisitorEntity entity) => new()
    {        
        Id = entity.Id,
        Score = entity.Score,
        RegistrationCount = entity.RegistrationCount,
    };
}
