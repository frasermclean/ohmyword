using OhMyWord.Domain.Models;
using OhMyWord.Infrastructure.Models.Entities;
using System.Net;

namespace OhMyWord.Domain.Extensions;

public static class PlayerEntityExtensions
{
    public static Player ToPlayer(this PlayerEntity entity, string connectionId) => new()
    {
        Id = entity.Id,
        ConnectionId = connectionId,
        UserId = entity.UserId,
        Score = entity.Score,
        RegistrationCount = entity.RegistrationCount,
        IpAddresses = entity.IpAddresses.Select(IPAddress.Parse)
    };
}
