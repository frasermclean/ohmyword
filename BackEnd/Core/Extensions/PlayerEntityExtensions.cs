using OhMyWord.Core.Models;
using OhMyWord.Data.Entities;

namespace OhMyWord.Core.Extensions;

public static class PlayerEntityExtensions
{
    public static Player ToModel(this PlayerEntity entity) => new()
    {
        Id = Guid.TryParse(entity.Id, out var id) ? id : Guid.Empty,
        VisitorId = entity.VisitorId,
        Score = entity.Score,
        RegistrationCount = entity.RegistrationCount,
    };
}
