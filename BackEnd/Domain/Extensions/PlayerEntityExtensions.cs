using OhMyWord.Domain.Models;
using OhMyWord.Infrastructure.Models.Entities;
using System.Net;

namespace OhMyWord.Domain.Extensions;

public static class PlayerEntityExtensions
{
    public static Player ToPlayer(this PlayerEntity entity, string connectionId, string visitorId,
        IPAddress ipAddress) => new()
    {
        Id = Guid.TryParse(entity.Id, out var id) ? id : Guid.Empty,
        Name = entity.UserId.HasValue ? string.Empty : "Guest", // TODO: Get name from user service
        ConnectionId = connectionId,
        UserId = entity.UserId,
        Score = entity.Score,
        RegistrationCount = entity.RegistrationCount,
        VisitorId = visitorId,
        IpAddress = ipAddress
    };
}
