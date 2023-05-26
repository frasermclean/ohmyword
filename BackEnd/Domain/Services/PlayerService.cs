using OhMyWord.Domain.Extensions;
using OhMyWord.Domain.Models;
using OhMyWord.Infrastructure.Models.Entities;
using OhMyWord.Infrastructure.Services;
using System.Net;

namespace OhMyWord.Domain.Services;

public interface IPlayerService
{
    Task<Player?> GetPlayerByIdAsync(Guid playerId, string? connectionId = default, string? visitorId = default);

    Task<Player> CreatePlayerAsync(Guid playerId, string connectionId, string visitorId, IPAddress ipAddress,
        Guid? userId);

    Task PatchPlayerRegistrationAsync(Player player, string visitorId, IPAddress ipAddress);

    Task IncrementPlayerScoreAsync(Guid playerId, int points);
}

public class PlayerService : IPlayerService
{
    private readonly IPlayerRepository playerRepository;

    public PlayerService(IPlayerRepository playerRepository)
    {
        this.playerRepository = playerRepository;
    }

    public async Task<Player?> GetPlayerByIdAsync(Guid playerId, string? connectionId, string? visitorId)
    {
        var entity = await playerRepository.GetPlayerByIdAsync(playerId);
        return entity?.ToPlayer(connectionId, visitorId);
    }

    public async Task<Player> CreatePlayerAsync(Guid playerId, string connectionId, string visitorId,
        IPAddress ipAddress, Guid? userId)
    {
        var entity = await playerRepository.CreatePlayerAsync(new PlayerEntity
        {
            Id = playerId.ToString(),
            UserId = userId,
            VisitorIds = new[] { visitorId },
            IpAddresses = new[] { ipAddress.ToString() }
        });

        return entity.ToPlayer(connectionId, visitorId);
    }

    public async Task PatchPlayerRegistrationAsync(Player player, string visitorId, IPAddress ipAddress)
    {
        await playerRepository.IncrementRegistrationCountAsync(player.Id);

        // patch ip address
        if (!player.IpAddresses.Contains(ipAddress))
            await playerRepository.AddIpAddressAsync(player.Id, ipAddress.ToString());

        // patch visitor id
        if (!player.VisitorIds.Contains(visitorId))
            await playerRepository.AddVisitorIdAsync(player.Id, visitorId);
    }

    public Task IncrementPlayerScoreAsync(Guid playerId, int points) =>
        playerRepository.IncrementScoreAsync(playerId, points);
}
