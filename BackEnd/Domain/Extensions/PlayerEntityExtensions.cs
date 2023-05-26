using OhMyWord.Domain.Models;
using OhMyWord.Infrastructure.Models.Entities;
using System.Net;

namespace OhMyWord.Domain.Extensions;

public static class PlayerEntityExtensions
{
    public static Player ToPlayer(this PlayerEntity entity, string? connectionId = default, string? visitorId = default)
        => new()
        {
            Id = Guid.TryParse(entity.Id, out var id) ? id : Guid.Empty,
            ConnectionId = connectionId,            
            UserId = entity.UserId,
            Score = entity.Score,
            RegistrationCount = entity.RegistrationCount,
            VisitorIds = entity.VisitorIds,
            IpAddresses = entity.IpAddresses.Select(IPAddress.Parse)
        };
}
