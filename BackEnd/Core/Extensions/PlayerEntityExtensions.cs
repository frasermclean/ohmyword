using OhMyWord.Core.Models;
using OhMyWord.Data.Entities;

namespace OhMyWord.Core.Extensions;

public static class VisitorEntityExtensions
{
    public static Visitor ToVisitor(this VisitorEntity entity) => new()
    {        
        Id = entity.Id,
        Score = entity.Score,
        RegistrationCount = entity.RegistrationCount,
    };
}
